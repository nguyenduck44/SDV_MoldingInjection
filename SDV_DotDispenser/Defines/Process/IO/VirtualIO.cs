using EQX.Core.InOut;
using EQX.InOut;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace SDV_DotDispenser.Defines
{
    public class VirtualIO
    {
        public VirtualIO(IEnumerable<IDInputDevice> processInputs,
                         IEnumerable<IDOutputDevice> processOutputs)
        {
            var procInList = processInputs.ToList();
            var procOutList = processOutputs.ToList();

            LeftStageProcInput = (IDInputDevice<EStageProcInput>?)
               procInList.First(pI => pI.Name == "LeftStageProcInput")!;

            LeftStageProcOutput = (IDOutputDevice<EStageProcOutput>?)
               procOutList.First(pI => pI.Name == "LeftStageProcOutput")!;

            RightStageProcInput = (IDInputDevice<EStageProcInput>?)
               procInList.First(pI => pI.Name == "RightStageProcInput")!;

            RightStageProcOutput = (IDOutputDevice<EStageProcOutput>?)
               procOutList.First(pI => pI.Name == "RightStageProcOutput")!;

            CleanProcInput = (IDInputDevice<ECleanProcInput>?)
               procInList.First(pI => pI.Name == "CleanProcInput")!;

            CleanProcOutput = (IDOutputDevice<ECleanProcOutput>?)
               procOutList.First(pI => pI.Name == "CleanProcOutput")!;

            DispenserProcInput = (IDInputDevice<EDispenserProcInput>?)
              procInList.First(pI => pI.Name == "DispenserProcInput")!;

            DispenserProcOutput = (IDOutputDevice<EDispenserProcOutput>?)
               procOutList.First(pI => pI.Name == "DispenserProcOutput")!;

            FinalInspectProcInput = (IDInputDevice<EFinalInspectProcInput>?)
              procInList.First(pI => pI.Name == "FinalInspectProcInput")!;

            FinalInspectProcOutput = (IDOutputDevice<EFinalInspectProcOutput>?)
               procOutList.First(pI => pI.Name == "FinalInspectProcOutput")!;

            UVProcInput = (IDInputDevice<EUVProcInput>?)
              procInList.First(pI => pI.Name == "UVProcInput")!;

            UVProcOutput = (IDOutputDevice<EUVProcOutput>?)
               procOutList.First(pI => pI.Name == "UVProcOutput")!;

            TransferProcInput = (IDInputDevice<ETransferProcInput>?)
                procInList.First(pI => pI.Name == "TransferProcessInput")!;

            TransferProcOutput = (IDOutputDevice<ETransferProcOutput>?)
                procOutList.First(pO => pO.Name == "TransferProcessOutput")!;
        }

        public IDInputDevice<EStageProcInput> LeftStageProcInput { get; }
        public IDOutputDevice<EStageProcOutput> LeftStageProcOutput { get; }

        public IDInputDevice<EStageProcInput> RightStageProcInput { get; }
        public IDOutputDevice<EStageProcOutput> RightStageProcOutput { get; }

        public IDInputDevice<ECleanProcInput> CleanProcInput { get; }
        public IDOutputDevice<ECleanProcOutput> CleanProcOutput { get; }

        public IDInputDevice<EDispenserProcInput> DispenserProcInput { get; }
        public IDOutputDevice<EDispenserProcOutput> DispenserProcOutput { get; }

        public IDInputDevice<EFinalInspectProcInput> FinalInspectProcInput { get; }
        public IDOutputDevice<EFinalInspectProcOutput> FinalInspectProcOutput { get; }

        public IDInputDevice<EUVProcInput> UVProcInput { get; }
        public IDOutputDevice<EUVProcOutput> UVProcOutput { get; }

        public IDInputDevice<ETransferProcInput> TransferProcInput { get; }
        public IDOutputDevice<ETransferProcOutput> TransferProcOutput { get; }

        public void Initialize()
        {
            PropertyInfo[] properties = typeof(VirtualIO).GetProperties();

            foreach (var prop in properties)
            {
                if (prop.PropertyType.GetGenericTypeDefinition() == typeof(IDInputDevice<>))
                {
                    ((IDInputDevice)(prop.GetValue(this))!).Initialize();
                }
                if (prop.PropertyType.GetGenericTypeDefinition() == typeof(IDOutputDevice<>))
                {
                    ((IDOutputDevice)(prop.GetValue(this))!).Initialize();
                }
            }
        }

        public void Mappings()
        {
            LeftStageProcInput[EStageProcInput.Dispenser_ZAxis_AtOrigin]
                .MapTo(DispenserProcOutput[EDispenserProcOutput.ZAxis_AtOrigin]);
            LeftStageProcInput[EStageProcInput.Transfer_ZAxis_AtOrigin]
                .MapTo(DispenserProcOutput[EDispenserProcOutput.ZAxis_AtOrigin]);

            RightStageProcInput[EStageProcInput.Dispenser_ZAxis_AtOrigin]
                .MapTo(DispenserProcOutput[EDispenserProcOutput.ZAxis_AtOrigin]);
            RightStageProcInput[EStageProcInput.Transfer_ZAxis_AtOrigin]
                .MapTo(DispenserProcOutput[EDispenserProcOutput.ZAxis_AtOrigin]);
        }
    }
}
