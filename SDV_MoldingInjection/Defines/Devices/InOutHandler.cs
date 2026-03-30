using EQX.Core.Common;
using EQX.Core.Communication;
using EQX.Core.Process;
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
        private readonly Processes _processes;
        private readonly Devices _devices;
        private readonly object _lockObject = new object();
        private string messageBuffer = string.Empty;

        private IProcess<ESequence> RootProcess => _processes.RootProcess;
        private InjectProcess InjectProcess => _processes.All.OfType<InjectProcess>().First();

        public InOutHandler([FromKeyedServices("InOutHandlerSerialCommunication")] SerialCommunicator serialCommunicator,
            RecipeSelector recipeSelector,
            Processes processes,
            Devices devices)
        {
            _serialCommunicator = serialCommunicator;
            _recipeSelector = recipeSelector;
            _processes = processes;
            _devices = devices;
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
            bool isChamberOpen = _devices.Cylinders.ChamberOpenClose.IsForward;
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
            if(RootProcess.ProcessMode != EQX.Core.Sequence.EProcessMode.Run ||
                RootProcess.Sequence != ESequence.AutoRun)
            {
                return false;
            }

            if(InjectProcess.Sequence != ESequence.Loading ||
               InjectProcess.Step.RunStep != (int)EMoldProcLoadingUnloadingStep.Wait_InOutHandlerStart_Request)
            {
                return false;
            }

            return true;
        }
    }
}
