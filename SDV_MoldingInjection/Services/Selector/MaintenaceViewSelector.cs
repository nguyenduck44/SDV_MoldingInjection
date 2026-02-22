using EQX.Core.Common;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.Recipe;
using System.Windows;
using System.Windows.Controls;

namespace SDV_MoldingInjection.Services
{
    public class MaintenaceViewSelector : DataTemplateSelector
    {
        public DataTemplate ManualViewTemplate { get; set; }
        public DataTemplate TeachViewTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var vm = item as MaintenanceViewModel<ESemiSequence, RecipeList>;
            if (vm == null) return null;

            // Logic của bạn ở đây
            return vm.MaintenanceView == EMaintenanceView.Manual ? ManualViewTemplate : TeachViewTemplate;
        }
    }
}
