using EQX.Core.Process;
using EQX.Process;
using SDV_DotDispenser.Defines;
using SDV_DotDispenser.MVVM.ViewModels;
using SDV_DotDispenser.Process;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

namespace SDV_DotDispenser.MVVM.Views
{
    /// <summary>
    /// Interaction logic for OriginView.xaml
    /// </summary>
    public partial class OriginView : UserControl
    {
        public OriginView()
        {
            InitializeComponent();
        }
        private void Border_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is OriginViewModel originVM == false) return;
            if (sender is Border border == false) return;
            if (border.DataContext is IProcess<ESequence> process == false) return;

            bool currentValue = process.IsOriginOrInitSelected;
            process.IsOriginOrInitSelected = !currentValue;
        }

        private void root_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is OriginViewModel originVM == false) return;
            originVM.Processes.RootProcess.Childs!.ToList().ForEach(p => p.IsOriginOrInitSelected = false);
        }
    }
}
