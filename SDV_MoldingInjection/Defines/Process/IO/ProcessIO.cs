using EQX.Core.InOut;
using EQX.InOut;
using System.Reflection;

namespace SDV_MoldingInjection.Defines
{
    public partial class ProcessIO
    {
        public ProcessIO(IEnumerable<IDInputDevice> processInputs,
                         IEnumerable<IDOutputDevice> processOutputs)
        {
            var procInList = processInputs.ToList();
            var procOutList = processOutputs.ToList();

            InjectProcInput = (IDInputDevice<EInjectProcInput>?)
               procInList.First(pI => pI.Name == "MoldProcInput")!;

            InjectProcOutput = (IDOutputDevice<EInjectProcOutput>?)
               procOutList.First(pI => pI.Name == "MoldProcOutput")!;

            DryPumpProcInput = (IDInputDevice<EDryPumpProcInput>?)
               procInList.First(pI => pI.Name == "DryPumpProcInput")!;

            DryPumpProcOutput = (IDOutputDevice<EDryPumpProcOutput>?)
               procOutList.First(pI => pI.Name == "DryPumpProcOutput")!;

            SPDHead1_ProcInput = (IDInputDevice<ESPDHeadProcInput>?)
               procInList.First(pI => pI.Name == "SPDHead1_ProcInput")!;

            SPDHead1_ProcOutput = (IDOutputDevice<ESPDHeadProcOutput>?)
               procOutList.First(pI => pI.Name == "SPDHead1_ProcOutput")!;

            SPDHead2_ProcInput = (IDInputDevice<ESPDHeadProcInput>?)
               procInList.First(pI => pI.Name == "SPDHead2_ProcInput")!;

            SPDHead2_ProcOutput = (IDOutputDevice<ESPDHeadProcOutput>?)
               procOutList.First(pI => pI.Name == "SPDHead2_ProcOutput")!;

            SPDHead3_ProcInput = (IDInputDevice<ESPDHeadProcInput>?)
               procInList.First(pI => pI.Name == "SPDHead3_ProcInput")!;

            SPDHead3_ProcOutput = (IDOutputDevice<ESPDHeadProcOutput>?)
               procOutList.First(pI => pI.Name == "SPDHead3_ProcOutput")!;

            SPDHead4_ProcInput = (IDInputDevice<ESPDHeadProcInput>?)
               procInList.First(pI => pI.Name == "SPDHead4_ProcInput")!;

            SPDHead4_ProcOutput = (IDOutputDevice<ESPDHeadProcOutput>?)
               procOutList.First(pI => pI.Name == "SPDHead4_ProcOutput")!;
        }

        public IDInputDevice<EInjectProcInput> InjectProcInput { get; }
        public IDOutputDevice<EInjectProcOutput> InjectProcOutput { get; }

        public IDInputDevice<EDryPumpProcInput> DryPumpProcInput { get; }
        public IDOutputDevice<EDryPumpProcOutput> DryPumpProcOutput { get; }

        public IDInputDevice<ESPDHeadProcInput> SPDHead1_ProcInput { get; }
        public IDOutputDevice<ESPDHeadProcOutput> SPDHead1_ProcOutput { get; }

        public IDInputDevice<ESPDHeadProcInput> SPDHead2_ProcInput { get; }
        public IDOutputDevice<ESPDHeadProcOutput> SPDHead2_ProcOutput { get; }

        public IDInputDevice<ESPDHeadProcInput> SPDHead3_ProcInput { get; }
        public IDOutputDevice<ESPDHeadProcOutput> SPDHead3_ProcOutput { get; }

        public IDInputDevice<ESPDHeadProcInput> SPDHead4_ProcInput { get; }
        public IDOutputDevice<ESPDHeadProcOutput> SPDHead4_ProcOutput { get; }

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
            InjectProcInput[EInjectProcInput.DryPump_VacuumDone]
                .MapTo(DryPumpProcOutput[EDryPumpProcOutput.ChamberVacuumSuccess]);
            InjectProcInput[EInjectProcInput.VentComplete]
                .MapTo(DryPumpProcOutput[EDryPumpProcOutput.VentComplete]);
            InjectProcInput[EInjectProcInput.SPDHead1_WorkDone]
                .MapTo(SPDHead1_ProcOutput[ESPDHeadProcOutput.InjectFinish]);
            InjectProcInput[EInjectProcInput.SPDHead2_WorkDone]
                .MapTo(SPDHead2_ProcOutput[ESPDHeadProcOutput.InjectFinish]);
            InjectProcInput[EInjectProcInput.SPDHead3_WorkDone]
                .MapTo(SPDHead3_ProcOutput[ESPDHeadProcOutput.InjectFinish]);
            InjectProcInput[EInjectProcInput.SPDHead4_WorkDone]
                .MapTo(SPDHead4_ProcOutput[ESPDHeadProcOutput.InjectFinish]);
            InjectProcInput[EInjectProcInput.SPDHead1_OriginDone]
                .MapTo(SPDHead1_ProcOutput[ESPDHeadProcOutput.OriginDone]);
            InjectProcInput[EInjectProcInput.SPDHead2_OriginDone]
                .MapTo(SPDHead2_ProcOutput[ESPDHeadProcOutput.OriginDone]);
            InjectProcInput[EInjectProcInput.SPDHead3_OriginDone]
                .MapTo(SPDHead3_ProcOutput[ESPDHeadProcOutput.OriginDone]);
            InjectProcInput[EInjectProcInput.SPDHead4_OriginDone]
                .MapTo(SPDHead4_ProcOutput[ESPDHeadProcOutput.OriginDone]);
            InjectProcInput[EInjectProcInput.SPDHead1_InjectAddTailDone]
                .MapTo(SPDHead1_ProcOutput[ESPDHeadProcOutput.InjectAddTailFinish]);
            InjectProcInput[EInjectProcInput.SPDHead2_InjectAddTailDone]
                .MapTo(SPDHead2_ProcOutput[ESPDHeadProcOutput.InjectAddTailFinish]);
            InjectProcInput[EInjectProcInput.SPDHead3_InjectAddTailDone]
                .MapTo(SPDHead3_ProcOutput[ESPDHeadProcOutput.InjectAddTailFinish]);
            InjectProcInput[EInjectProcInput.SPDHead4_InjectAddTailDone]
                .MapTo(SPDHead4_ProcOutput[ESPDHeadProcOutput.InjectAddTailFinish]);
            InjectProcInput[EInjectProcInput.Wait_SPDHead1_InjectAddTail]
                .MapTo(SPDHead1_ProcOutput[ESPDHeadProcOutput.ZAxisUpForInjectAddTailRequest]);
            InjectProcInput[EInjectProcInput.Wait_SPDHead2_InjectAddTail]
                .MapTo(SPDHead2_ProcOutput[ESPDHeadProcOutput.ZAxisUpForInjectAddTailRequest]);
            InjectProcInput[EInjectProcInput.Wait_SPDHead3_InjectAddTail]
                .MapTo(SPDHead3_ProcOutput[ESPDHeadProcOutput.ZAxisUpForInjectAddTailRequest]);
            InjectProcInput[EInjectProcInput.Wait_SPDHead4_InjectAddTail]
                .MapTo(SPDHead4_ProcOutput[ESPDHeadProcOutput.ZAxisUpForInjectAddTailRequest]);
            InjectProcInput[EInjectProcInput.SPDHead1_DotWeightingDone]
                .MapTo(SPDHead1_ProcOutput[ESPDHeadProcOutput.SPDHeadDotWeightingDone]);
            InjectProcInput[EInjectProcInput.SPDHead2_DotWeightingDone]
                .MapTo(SPDHead2_ProcOutput[ESPDHeadProcOutput.SPDHeadDotWeightingDone]);
            InjectProcInput[EInjectProcInput.SPDHead3_DotWeightingDone]
                .MapTo(SPDHead3_ProcOutput[ESPDHeadProcOutput.SPDHeadDotWeightingDone]);
            InjectProcInput[EInjectProcInput.SPDHead4_DotWeightingDone]
                .MapTo(SPDHead4_ProcOutput[ESPDHeadProcOutput.SPDHeadDotWeightingDone]);

            DryPumpProcInput[EDryPumpProcInput.Vacuum_WorkRequest]
                .MapTo(InjectProcOutput[EInjectProcOutput.DryPump_VacuumRequest]);

            SPDHead1_ProcInput[ESPDHeadProcInput.WorkRequest]
               .MapTo(InjectProcOutput[EInjectProcOutput.SPDHeadWorkRequest]);
            SPDHead2_ProcInput[ESPDHeadProcInput.WorkRequest]
               .MapTo(InjectProcOutput[EInjectProcOutput.SPDHeadWorkRequest]);
            SPDHead3_ProcInput[ESPDHeadProcInput.WorkRequest]
               .MapTo(InjectProcOutput[EInjectProcOutput.SPDHeadWorkRequest]);
            SPDHead4_ProcInput[ESPDHeadProcInput.WorkRequest]
               .MapTo(InjectProcOutput[EInjectProcOutput.SPDHeadWorkRequest]);
            SPDHead1_ProcInput[ESPDHeadProcInput.XYAxisInDummyPos]
               .MapTo(InjectProcOutput[EInjectProcOutput.XYAxisInDummyPos]);
            SPDHead2_ProcInput[ESPDHeadProcInput.XYAxisInDummyPos]
               .MapTo(InjectProcOutput[EInjectProcOutput.XYAxisInDummyPos]);
            SPDHead3_ProcInput[ESPDHeadProcInput.XYAxisInDummyPos]
               .MapTo(InjectProcOutput[EInjectProcOutput.XYAxisInDummyPos]);
            SPDHead4_ProcInput[ESPDHeadProcInput.XYAxisInDummyPos]
               .MapTo(InjectProcOutput[EInjectProcOutput.XYAxisInDummyPos]);

            SPDHead1_ProcInput[ESPDHeadProcInput.XYAxisInDotWeightingPos]
               .MapTo(InjectProcOutput[EInjectProcOutput.Request_H13DotWeighting]);
            SPDHead2_ProcInput[ESPDHeadProcInput.XYAxisInDotWeightingPos]
               .MapTo(InjectProcOutput[EInjectProcOutput.Request_H24DotWeighting]);
            SPDHead3_ProcInput[ESPDHeadProcInput.XYAxisInDotWeightingPos]
               .MapTo(InjectProcOutput[EInjectProcOutput.Request_H13DotWeighting]);
            SPDHead4_ProcInput[ESPDHeadProcInput.XYAxisInDotWeightingPos]
               .MapTo(InjectProcOutput[EInjectProcOutput.Request_H24DotWeighting]);
            SPDHead1_ProcInput[ESPDHeadProcInput.ZAxis_InjectAddTail_Ready]
               .MapTo(InjectProcOutput[EInjectProcOutput.SPDHead_InjectAddTail_Request]);
            SPDHead2_ProcInput[ESPDHeadProcInput.ZAxis_InjectAddTail_Ready]
               .MapTo(InjectProcOutput[EInjectProcOutput.SPDHead_InjectAddTail_Request]);
            SPDHead3_ProcInput[ESPDHeadProcInput.ZAxis_InjectAddTail_Ready]
               .MapTo(InjectProcOutput[EInjectProcOutput.SPDHead_InjectAddTail_Request]);
            SPDHead4_ProcInput[ESPDHeadProcInput.ZAxis_InjectAddTail_Ready]
               .MapTo(InjectProcOutput[EInjectProcOutput.SPDHead_InjectAddTail_Request]);
        }
    }
}
