using EQX.UI.Controls;
using System.Windows.Controls;
using System.Windows.Input;

namespace SDV_MoldingInjection.MVVM.Views
{
    /// <summary>
    /// Interaction logic for SPDHeadAllManualView.xaml
    /// </summary>
    public partial class SPDHeadAllManualView : UserControl
    {
        public SPDHeadAllManualView()
        {
            InitializeComponent();
        }

        private void TextBox_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBox textBox == false) return;

            if (int.TryParse(textBox.Text, out int currentValue) == false) return;

            DataEditor dataEditor = new DataEditor(currentValue, null);
            bool? dialogResult = dataEditor.ShowDialog();

            if (dialogResult == true)
            {
                textBox.Text = dataEditor.NewValue.ToString();
            }
        }
    }
}
