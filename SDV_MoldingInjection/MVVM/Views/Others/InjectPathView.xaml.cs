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

            DataEditor dataEditor = new DataEditor(Convert.ToDouble(textBox.Text), null);
            dataEditor.ShowDialog();

            textBox.Text = dataEditor.NewValue.ToString();
        }
    }
}
