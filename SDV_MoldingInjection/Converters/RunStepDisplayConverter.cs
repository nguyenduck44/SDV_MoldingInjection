using SDV_MoldingInjection.Defines;
using System.Globalization;
using System.Windows.Data;

namespace SDV_MoldingInjection.Converters
{
    public class RunStepDisplayConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 3) return string.Empty;

            var processName = values[0]?.ToString() ?? string.Empty;
            if (values[1] is not ESequence sequence) return values[2]?.ToString() ?? string.Empty;
            if (!int.TryParse(values[2]?.ToString(), out int runStep)) return values[2]?.ToString() ?? string.Empty;

            var enumType = ResolveStepEnumType(processName, sequence);
            if (enumType == null) return runStep.ToString();

            if (!Enum.IsDefined(enumType, runStep)) return runStep.ToString();
            return Enum.GetName(enumType, runStep) ?? runStep.ToString();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return new object[] { Binding.DoNothing, Binding.DoNothing, Binding.DoNothing };
        }

        private static Type? ResolveStepEnumType(string processName, ESequence sequence)
        {
            return processName switch
            {
                "Inject" => sequence switch
                {
                    ESequence.AutoRun => typeof(EMoldProcAutoRunStep),
                    ESequence.Loading or ESequence.Unloading => typeof(EMoldProcLoadingUnloadingStep),
                    ESequence.ResinInject => typeof(EMoldProcResinInjectStep),
                    ESequence.ResinInjectAddTail => typeof(EMoldProcResinInjectAddTailStep),
                    ESequence.DummyShot => typeof(EMoldProcDummyShotStep),
                    ESequence.NeedleCleaning => typeof(EMoldProcNeedleCleaningStep),
                    ESequence.BubbleRemove => typeof(EMoldProcDummyShotStep),
                    ESequence.DotWeighting => typeof(EMoldProcDotWeightingStep),
                    ESequence.HeadAssemble_Step1 or ESequence.HeadDisassemble => typeof(EMoldProcDummyShotStep),
                    _ => null
                },
                "DryPump" => sequence switch
                {
                    ESequence.AutoRun => typeof(EDryPumpProcResinInjectStep),
                    ESequence.ResinInject => typeof(EDryPumpProcResinInjectStep),
                    _ => null
                },
                "SPDHead1" => sequence switch
                {
                    ESequence.ResinInject => typeof(ESPDHeadProcCommonStep),
                    ESequence.DummyShot => typeof(ESPDHeadProcCommonStep),
                    ESequence.BubbleRemove => typeof(ESPDHeadProcCommonStep),
                    ESequence.DotWeighting => typeof(ESPDHeadProcDotWeightingStep),
                    ESequence.HeadAssemble_Step1 or ESequence.HeadDisassemble => typeof(ESPDHeadProcAssembleDisAssembleStep),
                    _ => null
                },
                "SPDHead2" => sequence switch
                {
                    ESequence.ResinInject => typeof(ESPDHeadProcCommonStep),
                    ESequence.DummyShot => typeof(ESPDHeadProcCommonStep),
                    ESequence.BubbleRemove => typeof(ESPDHeadProcCommonStep),
                    ESequence.DotWeighting => typeof(ESPDHeadProcDotWeightingStep),
                    ESequence.HeadAssemble_Step1 or ESequence.HeadDisassemble => typeof(ESPDHeadProcAssembleDisAssembleStep),
                    _ => null
                },
                "SPDHead3" => sequence switch
                {
                    ESequence.ResinInject => typeof(ESPDHeadProcCommonStep),
                    ESequence.DummyShot => typeof(ESPDHeadProcCommonStep),
                    ESequence.BubbleRemove => typeof(ESPDHeadProcCommonStep),
                    ESequence.DotWeighting => typeof(ESPDHeadProcDotWeightingStep),
                    ESequence.HeadAssemble_Step1 or ESequence.HeadDisassemble => typeof(ESPDHeadProcAssembleDisAssembleStep),
                    _ => null
                },
                "SPDHead4" => sequence switch
                {
                    ESequence.ResinInject => typeof(ESPDHeadProcCommonStep),
                    ESequence.DummyShot => typeof(ESPDHeadProcCommonStep),
                    ESequence.BubbleRemove => typeof(ESPDHeadProcCommonStep),
                    ESequence.DotWeighting => typeof(ESPDHeadProcDotWeightingStep),
                    ESequence.HeadAssemble_Step1 or ESequence.HeadDisassemble => typeof(ESPDHeadProcAssembleDisAssembleStep),
                    _ => null
                },
                "Monitoring" => sequence switch
                {
                    ESequence.MoveMultiPoint => typeof(EMonitoringProcessMoveMultiPointStep),
                    _ => null
                },
                _ => null
            };
        }
    }
}
