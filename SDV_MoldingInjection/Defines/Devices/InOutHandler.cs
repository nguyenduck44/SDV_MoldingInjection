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
        private readonly object _lockObject = new object();
        private string messageBuffer = string.Empty;

        private IProcess<ESequence> RootProcess => _processes.RootProcess;
        private IProcess<ESequence> InjectProcess => _processes.All.OfType<InjectProcess>().First();

        public InOutHandler([FromKeyedServices("InOutHandlerSerialCommunication")] SerialCommunicator serialCommunicator,
            RecipeSelector recipeSelector,
            Processes processes)
        {
            _serialCommunicator = serialCommunicator;
            _recipeSelector = recipeSelector;
            _processes = processes;
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

            string command = messageBuffer.Substring(1, 4);

            switch (command)
            {
                case "CRRD": // READY CHECK
                    Handler_ReadyCheck_Received(messageBuffer);
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
