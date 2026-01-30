using EQX.Core.InOut;
using EQX.InOut;
using Microsoft.Extensions.DependencyInjection;

namespace EQX.TemplateProject.Defines
{
    public class VirtualIO
    {
        public VirtualIO([FromKeyedServices("TransferProcessInput")] IDInputDevice transferProcessInput,
                         [FromKeyedServices("TransferProcessOutput")] IDOutputDevice transferProcessOutput,
                         [FromKeyedServices("ShuttleProcessInput")] IDInputDevice shuttleProcessInput,
                         [FromKeyedServices("ShuttleProcessOutput")] IDOutputDevice shuttleProcessOutput)
        {
            TransferProcessInput = transferProcessInput;
            TransferProcessOutput = transferProcessOutput;
            ShuttleProcessInput = shuttleProcessInput;
            ShuttleProcessOutput = shuttleProcessOutput;
        }

        public IDInputDevice TransferProcessInput { get; }
        public IDOutputDevice TransferProcessOutput { get; }
        public IDInputDevice ShuttleProcessInput { get; }
        public IDOutputDevice ShuttleProcessOutput { get; }

        public void Initialize()
        {
            TransferProcessInput.Initialize();
            TransferProcessOutput.Initialize();

            ShuttleProcessInput.Initialize();
            ShuttleProcessOutput.Initialize();
        }

        public void Mappings()
        {
            TransferProcessInput[ETransferProcessInput.SHUTTLE_REQUEST_LOAD]
                .MapTo(ShuttleProcessOutput[EShuttleProcessOutput.SHUTTLE_REQUEST_LOAD]);

            ShuttleProcessInput[EShuttleProcessInput.TRANSFER_LOAD_DONE]
                .MapTo(TransferProcessOutput[ETransferProcessOutput.TRANSFER_LOAD_DONE]);
        }
    }
}
