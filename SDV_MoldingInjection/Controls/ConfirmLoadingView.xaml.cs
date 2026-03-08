using SDV_MoldingInjection.Defines;
using System.ComponentModel;
using System.Windows;

namespace SDV_MoldingInjection.Controls
{
    /// <summary>
    /// Interaction logic for ConfirmLoadingView.xaml
    /// </summary>
    public partial class ConfirmLoadingView : Window
    {
        public ConfirmLoadingView()
        {
            InitializeComponent();
        }

        public bool IsLoadingFinish {  get; set; }
        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            IsLoadingFinish = false;
            DialogResult = false;
        }

        private void ButtonConfirm_Click(object sender, RoutedEventArgs e)
        {
            IsLoadingFinish = true;
            DialogResult = true;
        }
    }
}
