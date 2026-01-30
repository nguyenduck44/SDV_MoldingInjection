using EQX.Core.InOut;
using EQX.InOut;
using Microsoft.Extensions.DependencyInjection;

namespace SDV_DotDispenser.Defines
{
    public class VirtualIO
    {
        public VirtualIO([FromKeyedServices("TransferProcessInput")] IDInputDevice transferProcessInput,
                         [FromKeyedServices("TransferProcessOutput")] IDOutputDevice transferProcessOutput)
        {
            TransferProcessInput = transferProcessInput;
            TransferProcessOutput = transferProcessOutput;
        }

        public IDInputDevice TransferProcessInput { get; }
        public IDOutputDevice TransferProcessOutput { get; }

        public void Initialize()
        {
            TransferProcessInput.Initialize();
            TransferProcessOutput.Initialize();
        }

        public void Mappings()
        {
        }
    }
}
