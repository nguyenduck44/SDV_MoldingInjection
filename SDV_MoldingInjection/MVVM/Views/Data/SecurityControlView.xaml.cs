using EQX.UI.Controls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SDV_MoldingInjection.MVVM.Views
{
    /// <summary>
    /// Interaction logic for SecurityControlView.xaml
    /// </summary>
    public partial class SecurityControlView : UserControl
    {
        public SecurityControlView()
        {
            InitializeComponent();
        }

        private void keyBoardBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not TextBox textBox)
            {
                return;
            }

            VirtualKeyboard virtualKeyboard = new VirtualKeyboard(false)
            {
                Height = 400,
                Width = 1024,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Owner = Window.GetWindow(this),
                InputText = textBox.Text
            };
            if (virtualKeyboard.ShowDialog() == true)
            {
                textBox.Text = virtualKeyboard.InputText;
            }
            e.Handled = true;
        }
    }
}
