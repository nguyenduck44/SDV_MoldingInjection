using EQX.Core.Common;
using EQX.Core.Communication;
using EQX.Core.Process;
using EQX.Core.Sequence;
using EQX.InOut;
using log4net;
using Microsoft.Extensions.DependencyInjection;
using SDV_MoldingInjection.Process;
using SDV_MoldingInjection.Recipe;

namespace SDV_MoldingInjection.Defines
{
    public class InOutHandler : IHandleConnection, ILogable
    {
        private readonly SerialCommunicator _serialCommunicator;
        private readonly RecipeSelector _recipeSelector;
        private readonly IServiceProvider _serviceProvider;
        private readonly Devices _devices;
        private readonly MachineStatus _machineStatus;
        private readonly object _lockObject = new object();
        private string messageBuffer = string.Empty;

        private Processes _processes => _serviceProvider.GetRequiredService<Processes>();
        private IProcess<ESequence> RootProcess => _processes.RootProcess;
        private InjectProcess InjectProcess => _processes.All.OfType<InjectProcess>().First();

        public InOutHandler([FromKeyedServices("InOutHandlerSerialCommunication")] SerialCommunicator serialCommunicator,
            RecipeSelector recipeSelector,
            IServiceProvider serviceProvider,
            Devices devices,
            MachineStatus machineStatus)
        {
            _serialCommunicator = serialCommunicator;
            _recipeSelector = recipeSelector;
            _serviceProvider = serviceProvider;
            _devices = devices;
            _machineStatus = machineStatus;
        }

        public bool IsConnected => _serialCommunicator.IsConnected;

        public ILog Log => LogManager.GetLogger("InOutHandler");

        public bool Connect()
        {
            if (IsConnected)
            {
                return false;
            }

            lock (_lockObject)
            {
                try
                {
                    if (!_serialCommunicator.Connect())
                    {
                        return false;
                    }

                    _serialCommunicator.DataReceived += DataReceivedHandler;
                    return true;
                }
                catch
                {
                    _serialCommunicator.Disconnect();
                    return false;
                }
            }
        }

        public bool Disconnect()
        {
            _serialCommunicator.Disconnect();
            return true;
        }

        private void DataReceivedHandler(object? sender, EventArgs eventArgs)
        {
            string messageReceived = (string)sender;
            HandleMessageReceived(messageReceived);
        }

        private void HandleMessageReceived(string message)
        {
            if (message.First() == 0x02)
            {
                messageBuffer = message;
            }
            else
            {
                messageBuffer += message;
            }

            if (messageBuffer.Last() != 0x03)
            {
                return;
            }

            if (_recipeSelector.CurrentRecipe.OptionRecipe.InputTypeManual) return;

            if (messageBuffer.First() != 0x02 || messageBuffer.Last() != 0x03 || messageBuffer.Length < 6)
            {
                return;
            }

            string command = messageBuffer.Substring(1, 4);

            switch (command)
            {
                case "CRRD": // READY CHECK
                    Handler_ReadyCheck_Received(messageBuffer);
                    break;
                case "CRRN": //CHECK RUN
                    Handler_CheckRun_Received(messageBuffer);
                    break;
                case "CRCH": //CHECK PRODUCT
                    Handler_ProductCheck_Received(messageBuffer);
                    break;
                case "CRPT": //CHECK POSITION
                    Handler_PositionCheck_Received(messageBuffer);
                    break;
                case "CRUS": // HEAD USE
                    Handler_HeadUse_Received(messageBuffer);
                    break;
                case "CRST": // START
                    Handler_Start_Received(messageBuffer);
                    break;
                case "CRSP": // STOP
                    Handler_Stop_Received(messageBuffer);
                    break;
                case "CRSI": //SET IDLE PURGE MODE
                    Handler_SetIdlePurgeMode_Received(messageBuffer);
                    break;
                case "CRCI": //CLEAR ILDE PURGE MODE
                    Handler_ClearIdlePurgeMode_Received(messageBuffer);
                    break;
            }
        }

        private void Handler_ReadyCheck_Received(string message)
        {
            bool isMachineReady = MachineReadyCheck();

            byte[] buffer = new byte[] {
                0x02,
                0x43, 0x54, 0x52, 0x44,
                (byte)(isMachineReady == false ? 0x15 : 0x16),
                0x03
            };

            TransmitData(buffer);
        }

        private void Handler_CheckRun_Received(string message)
        {
            bool isOnIdlePurgeMode = RootProcess.ProcessMode == EQX.Core.Sequence.EProcessMode.Run && RootProcess.Sequence == ESequence.IdlePurge;
            bool isMachineRun = RootProcess.ProcessMode == EQX.Core.Sequence.EProcessMode.Run;

            bool isDoorOpen = _devices.Inputs.DoorClose == false;
            bool isMachineError = RootProcess.ProcessMode == EQX.Core.Sequence.EProcessMode.Alarm || RootProcess.ProcessMode == EQX.Core.Sequence.EProcessMode.ToAlarm ||
                                  RootProcess.ProcessMode == EQX.Core.Sequence.EProcessMode.Warning || RootProcess.ProcessMode == EQX.Core.Sequence.EProcessMode.ToWarning;
            byte[] buffer = new byte[] {
                0x02,
                0x43, 0x54, 0x52, 0x4E,
                (byte)(isOnIdlePurgeMode ? 0x17 : isMachineRun ? 0x15 : 0x16),
                (byte)(isMachineError ? 0x31 : isDoorOpen ? 0x32 : 0x30),
                0x03
            };

            TransmitData(buffer);
        }

        private void Handler_ProductCheck_Received(string message)
        {
            bool isChamberOpen = _devices.Cylinders.ChamberOpenClose.IsOpen();

            var opt = _recipeSelector.CurrentRecipe.OptionRecipe;
            var inj = InjectProcess;

            byte jig1Status = MapJigPairToProtocolStatus(opt.SkipHead12,
                inj.JigStatuses[0],
                _devices.Inputs.Jig1Detect.Value,
                _devices.Inputs.Jig2Detect.Value);

            byte jig2Status = MapJigPairToProtocolStatus(opt.SkipHead34,
                inj.JigStatuses[1],
                _devices.Inputs.Jig3Detect.Value,
                _devices.Inputs.Jig4Detect.Value);

            byte[] buffer = new byte[] {
                0x02,
                0x43, 0x54, 0x43, 0x48,
                jig1Status,
                jig2Status,
                (byte)(isChamberOpen ? 0x32 : 0x31),
                0x03
            };

            TransmitData(buffer);
        }

        private void Handler_PositionCheck_Received(string message)
        {
            if (_devices.Motions.StageYAxis.IsOnPosition(_recipeSelector.CurrentRecipe.InjectRecipe.YAxisReadyPos) == false) return;

            bool isChamberOpen = _devices.Cylinders.ChamberOpenClose.IsOpen();

            byte[] buffer = new byte[] {
                0x02,
                0x43, 0x54, 0x50, 0x54,
                (byte)(isChamberOpen ? 0x32 : 0x31),
                0x03
            };
            TransmitData(buffer);
        }

        private void Handler_HeadUse_Received(string message)
        {
            Response_HeadUse_Received();
            int head = message[5] - 0x31;
            bool isUse = message[6] == 0x16;

            if (head == 0) _recipeSelector.CurrentRecipe.OptionRecipe.SkipHead12 = isUse;
            else if (head == 1) _recipeSelector.CurrentRecipe.OptionRecipe.SkipHead34 = isUse;

            Handler_Send_HeadUse_Success(head);
        }

        private void Response_HeadUse_Received()
        {
            byte[] buffer = new byte[] {
                0x02,
                0x43, 0x54, 0x55, 0x53,
                0x03
            };

            TransmitData(buffer);
        }

        private void Handler_Send_HeadUse_Success(int head)
        {
            bool isUse = head == 0 ? _recipeSelector.CurrentRecipe.OptionRecipe.SkipHead12 : _recipeSelector.CurrentRecipe.OptionRecipe.SkipHead34;
            byte[] buffer = new byte[] {
                0x02,
                0x43, 0x54, 0x55, 0x45,
                head == 0 ? (byte)0x31 : (byte)0x32,
                isUse ? (byte)0x16 : (byte)0x15,
                0x03
            };

            TransmitData(buffer);
        }

        private void Handler_Start_Received(string message)
        {
            _machineStatus.InOutHandlerStartRequest = true;

            byte[] buffer = new byte[] {
                0x02,
                0x43, 0x54, 0x53, 0x54,
                0x03
            };

            TransmitData(buffer);
        }

        private void Handler_Stop_Received(string message)
        {
            _machineStatus.OPCommand = EOperationCommand.Stop;

            byte[] buffer = new byte[] {
                0x02,
                0x43, 0x54, 0x53, 0x50,
                0x03
            };

            TransmitData(buffer);
        }

        private void Handler_SetIdlePurgeMode_Received(string message)
        {
            _machineStatus.OPCommand = EOperationCommand.SemiAuto;
            _machineStatus.SemiAutoSequence = ESemiSequence.IdlePurge;

            byte[] buffer = new byte[] {
                0x02,
                0x43, 0x54, 0x53, 0x49,
                0x03
            };

            TransmitData(buffer);
        }

        private void Handler_ClearIdlePurgeMode_Received(string message)
        {
            _machineStatus.OPCommand = EOperationCommand.Stop;

            byte[] buffer = new byte[] {
                0x02,
                0x43, 0x54, 0x43, 0x49,
                0x03
            };

            TransmitData(buffer);
        }

        public void Handler_Send_End(EJigStatus[] jigStatuses)
        {
            if(jigStatuses.Length != 2) return;

            bool isChamberOpen = _devices.Cylinders.ChamberOpenClose.IsOpen();

            var opt = _recipeSelector.CurrentRecipe.OptionRecipe;
            var inj = InjectProcess;

            byte jig1Status = MapJigPairToProtocolStatus(opt.SkipHead12,
                inj.JigStatuses[0],
                _devices.Inputs.Jig1Detect.Value,
                _devices.Inputs.Jig2Detect.Value);

            byte jig2Status = MapJigPairToProtocolStatus(opt.SkipHead34,
                inj.JigStatuses[1],
                _devices.Inputs.Jig3Detect.Value,
                _devices.Inputs.Jig4Detect.Value);

            byte[] buffer = new byte[] {
                0x02,
                0x43, 0x54, 0x45, 0x4E,
                jig1Status,
                jig2Status,
                (byte)(isChamberOpen ? 0x32 : 0x31),
                0x03
            };
        }

        public void Handler_SendError(int errorCode)
        {
            byte[] buffer = new byte[] {
                0x02,
                0x43, 0x54, 0x45, 0x52,
                (byte)(0x30 + (errorCode/1000)),
                (byte)(0x30 + ((errorCode % 1000) /100)),
                (byte)(0x30 + ((errorCode % 100) /10)),
                (byte)(0x30 + (errorCode % 10)),
                0x03
            };
        }

        /// <summary>11–15 theo bảng 제품 상태 protocol.</summary>
        private byte MapJigPairToProtocolStatus(bool skipPair, EJigStatus jigStatus, bool jigDet1, bool jigDet2)
        {
            if (skipPair) return 0x15;
            if (!jigDet1 && !jigDet2) return 0x14;

            return jigStatus switch
            {
                EJigStatus.MoldingFinish => 0x12,
                EJigStatus.InMolding => 0x11,
                EJigStatus.Ready => 0x13,
                _ => 0x13,
            };
        }

        private void TransmitData(byte[] buffer)
        {
            if (_recipeSelector.CurrentRecipe.OptionRecipe.InputTypeManual) return;

            _serialCommunicator.Write(buffer);
        }

        private bool MachineReadyCheck()
        {
            if (RootProcess.ProcessMode != EQX.Core.Sequence.EProcessMode.Run ||
                RootProcess.Sequence != ESequence.AutoRun)
            {
                return false;
            }

            if (InjectProcess.Sequence != ESequence.Loading ||
               InjectProcess.Step.RunStep != (int)EMoldProcLoadingUnloadingStep.Wait_InOutHandlerStart_Request)
            {
                return false;
            }

            return true;
        }
    }
}
