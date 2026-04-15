using SDV_MoldingInjection.MVVM.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace SDV_MoldingInjection.MVVM.Views
{
    /// <summary>
    /// Interaction logic for ErrorView.xaml
    /// </summary>
    public partial class ErrorView : UserControl
    {
        public ErrorView()
        {
            InitializeComponent();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void ErrorListHistoryDataGrid_Sorting(object sender, DataGridSortingEventArgs e)
        {

        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is ErrorViewModel errorViewModel == false) return;

            await errorViewModel.LoadErrorData(errorViewModel.ErrorFilePathList);
        }
    }
}
