using SDV_MoldingInjection.MVVM.ViewModels;
using System.Windows;

namespace SDV_MoldingInjection.MVVM.Views
{
    public interface ILeakTestResultDialogService
    {
        void Show(LeakTestResultViewModel viewModel);
    }

    public class LeakTestResultDialogService : ILeakTestResultDialogService
    {
        public void Show(LeakTestResultViewModel viewModel)
        {
            var window = new LeakTestResultWindow(viewModel)
            {
                Owner = Application.Current?.MainWindow,
            };
            window.Show();
        }
    }

    public partial class LeakTestResultWindow : Window
    {
        public LeakTestResultWindow(LeakTestResultViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
