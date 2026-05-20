using SDV_MoldingInjection.Controls;
using SDV_MoldingInjection.MVVM.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SDV_MoldingInjection.MVVM.Views
{
    /// <summary>
    /// Interaction logic for MachineStatusAutoView.xaml
    /// </summary>
    public partial class MachineStatusAutoView : UserControl
    {
        public MachineStatusAutoView()
        {
            InitializeComponent();
        }

        private void ConfirmLoadingFinish_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var image = sender as IInputElement;
            if (image == null)
            {
                return;
            }

            if (this.DataContext is MachineStatusAutoViewModel machineStatusAutoViewModel == false) return;
            if (machineStatusAutoViewModel.RecipeSelector.CurrentRecipe.OptionRecipe.InputTypeAuto) return;

            Point clickPosInElement = e.GetPosition((IInputElement)sender);
            Point screenPos = (sender as Visual)!.PointToScreen(clickPosInElement);

            var dialog = new ConfirmLoadingView
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Left = screenPos.X,
                Top = screenPos.Y
            };

            dialog.ShowInTaskbar = false;
            dialog.Topmost = true;

            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                machineStatusAutoViewModel.MachineStatus.ConfirmLoadingFinish = dialog.IsLoadingFinish;
            }
        }
    }
}
