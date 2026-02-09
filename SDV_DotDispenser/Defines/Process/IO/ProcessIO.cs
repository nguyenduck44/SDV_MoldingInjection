using EQX.Core.InOut;
using EQX.InOut;
using System.Reflection;

namespace SDV_DotDispenser.Defines
{
    public class ProcessIO
    {
        public ProcessIO(IEnumerable<IDInputDevice> processInputs,
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

            PlasmaStageProcInput = (IDInputDevice<EPlasmaProcInput>?)
               procInList.First(pI => pI.Name == "PlasmaProcInput")!;

            PlasmaStageProcOutput = (IDOutputDevice<EPlasmaProcOutput>?)
               procOutList.First(pI => pI.Name == "PlasmaProcOutput")!;

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

        public IDInputDevice<EPlasmaProcInput> PlasmaStageProcInput { get; }
        public IDOutputDevice<EPlasmaProcOutput> PlasmaStageProcOutput { get; }

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
            PropertyInfo[] properties = typeof(ProcessIO).GetProperties();

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
            LeftStageProcInput[EStageProcInput.DISPENSER_Z_AT_ORIGIN]
                .MapTo(DispenserProcOutput[EDispenserProcOutput.DISPENSER_Z_AT_ORIGIN]);
            LeftStageProcInput[EStageProcInput.TRANSFER_Z_AT_ORIGIN]
                .MapTo(DispenserProcOutput[EDispenserProcOutput.DISPENSER_Z_AT_ORIGIN]);
            LeftStageProcInput[EStageProcInput.PLASMA_COVER_MOVE_DONE]
                .MapTo(PlasmaStageProcOutput[EPlasmaProcOutput.LEFT_PLASMA_COVER_MOVE_DONE]);
            LeftStageProcInput[EStageProcInput.DISPENSING_DONE]
                .MapTo(DispenserProcOutput[EDispenserProcOutput.LEFT_STAGE_DISPENSING_DONE]);
            LeftStageProcInput[EStageProcInput.UVCURE_DONE]
                .MapTo(UVProcOutput[EUVProcOutput.LEFT_STAGE_UVCURE_DONE]);
            LeftStageProcInput[EStageProcInput.FINAL_INSPECT_DONE]
                .MapTo(FinalInspectProcOutput[EFinalInspectProcOutput.LEFT_STAGE_FINAL_INSPECT_DONE]);
            LeftStageProcInput[EStageProcInput.TRANSFER_DONE]
                .MapTo(TransferProcOutput[ETransferProcOutput.LEFT_STAGE_TRANSFER_DONE]);

            RightStageProcInput[EStageProcInput.DISPENSER_Z_AT_ORIGIN]
                .MapTo(DispenserProcOutput[EDispenserProcOutput.DISPENSER_Z_AT_ORIGIN]);
            RightStageProcInput[EStageProcInput.TRANSFER_Z_AT_ORIGIN]
                .MapTo(DispenserProcOutput[EDispenserProcOutput.DISPENSER_Z_AT_ORIGIN]);
            RightStageProcInput[EStageProcInput.PLASMA_COVER_MOVE_DONE]
                .MapTo(PlasmaStageProcOutput[EPlasmaProcOutput.RIGHT_PLASMA_COVER_MOVE_DONE]);
            RightStageProcInput[EStageProcInput.DISPENSING_DONE]
                .MapTo(DispenserProcOutput[EDispenserProcOutput.RIGHT_STAGE_DISPENSING_DONE]);
            RightStageProcInput[EStageProcInput.UVCURE_DONE]
                .MapTo(UVProcOutput[EUVProcOutput.RIGHT_STAGE_UVCURE_DONE]);
            RightStageProcInput[EStageProcInput.FINAL_INSPECT_DONE]
                .MapTo(FinalInspectProcOutput[EFinalInspectProcOutput.RIGHT_STAGE_FINAL_INSPECT_DONE]);
            RightStageProcInput[EStageProcInput.TRANSFER_DONE]
                .MapTo(TransferProcOutput[ETransferProcOutput.RIGHT_STAGE_TRANSFER_DONE]);

            PlasmaStageProcInput[EPlasmaProcInput.LEFT_STAGE_REQ_PLASMA]
                .MapTo(LeftStageProcOutput[EStageProcOutput.STAGE_REQ_PLASMA]);
            PlasmaStageProcInput[EPlasmaProcInput.RIGHT_STAGE_REQ_PLASMA]
                .MapTo(RightStageProcOutput[EStageProcOutput.STAGE_REQ_PLASMA]);

            DispenserProcInput[EDispenserProcInput.LEFT_STAGE_REQ_DISPENSING]
                .MapTo(LeftStageProcOutput[EStageProcOutput.STAGE_REQ_DISPENSING]);
            DispenserProcInput[EDispenserProcInput.RIGHT_STAGE_REQ_DISPENSING]
                .MapTo(RightStageProcOutput[EStageProcOutput.STAGE_REQ_DISPENSING]);
            DispenserProcInput[EDispenserProcInput.CLEAN_PREPARE_DONE]
                .MapTo(CleanProcOutput[ECleanProcOutput.CLEAN_PREPARE_DONE]);

            CleanProcInput[ECleanProcInput.DISPENSER_REQ_CLEAN]
                .MapTo(DispenserProcOutput[EDispenserProcOutput.DISPENSER_REQ_CLEAN]);

            UVProcInput[EUVProcInput.LEFT_STAGE_REQ_UVCURE]
                .MapTo(LeftStageProcOutput[EStageProcOutput.STAGE_REQ_UVCURE]);
            UVProcInput[EUVProcInput.RIGHT_STAGE_REQ_UVCURE]
                .MapTo(RightStageProcOutput[EStageProcOutput.STAGE_REQ_UVCURE]);

            FinalInspectProcInput[EFinalInspectProcInput.LEFT_STAGE_REQ_FINAL_INSPECT]
                .MapTo(LeftStageProcOutput[EStageProcOutput.STAGE_REQ_FINAL_INSPECT]);
            FinalInspectProcInput[EFinalInspectProcInput.RIGHT_STAGE_REQ_FINAL_INSPECT]
                .MapTo(RightStageProcOutput[EStageProcOutput.STAGE_REQ_FINAL_INSPECT]);

            TransferProcInput[ETransferProcInput.LEFT_STAGE_REQ_TRANSFER]
                .MapTo(LeftStageProcOutput[EStageProcOutput.STAGE_REQ_TRANSFER]);
            TransferProcInput[ETransferProcInput.RIGHT_STAGE_REQ_TRANSFER]
                .MapTo(RightStageProcOutput[EStageProcOutput.STAGE_REQ_TRANSFER]);
        }
    }
}
