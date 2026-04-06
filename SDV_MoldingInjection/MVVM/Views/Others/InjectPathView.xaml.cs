using EQX.UI.Controls;
using System.Windows.Controls;
using System.Windows.Input;

namespace SDV_MoldingInjection.MVVM.Views
{
    /// <summary>
    /// Interaction logic for InjectPathView.xaml
    /// </summary>
    public partial class InjectPathView : UserControl
    {
        public InjectPathView()
        {
            InitializeComponent();
        }

        private void TextBox_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBox textBox == false) return;

            if (double.TryParse(textBox.Text, out double currentValue) == false) return;

            DataEditor dataEditor = new DataEditor(currentValue, null);
            bool? dialogResult = dataEditor.ShowDialog();

            if (dialogResult == true)
            {
                textBox.Text = dataEditor.NewValue.ToString();
            }
        }
    }
}
