using EQX.UI.Controls;
using SDV_DemoEQ.MVVM.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SDV_DemoEQ.MVVM.Views
{
    /// <summary>
    /// Interaction logic for LoginView.xaml
    /// </summary>
    public partial class LoginView : UserControl
    {
        public LoginView()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if(this.DataContext is LoginViewModel loginViewModel)
            {
                loginViewModel.LoginCommand.Execute(passwordBox.Password);
            }
        }

        private void passwordBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            VirtualKeyboard virtualKeyboard = new VirtualKeyboard();
            if(virtualKeyboard.ShowDialog() == true)
            {
                passwordBox.Password = virtualKeyboard.InputText;
            }    
        }
    }
}
