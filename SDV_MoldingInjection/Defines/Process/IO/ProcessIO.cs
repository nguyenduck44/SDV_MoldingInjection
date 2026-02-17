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

            MoldProcInput = (IDInputDevice<EMoldProcInput>?)
               procInList.First(pI => pI.Name == "MoldProcInput")!;

            MoldProcOutput = (IDOutputDevice<EMoldProcOutput>?)
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

        public IDInputDevice<EMoldProcInput> MoldProcInput { get; }
        public IDOutputDevice<EMoldProcOutput> MoldProcOutput { get; }

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
            
        }
    }
}
