using System;
using System.Windows.Controls;
using SDV_DotDispenser.MVVM.ViewModels;
using EQX.Core.Process;
using SDV_DotDispenser.Defines;

namespace SDV_DotDispenser.MVVM.Views
{
    /// <summary>
    /// Interaction logic for TeachView.xaml
    /// </summary>
    public partial class TeachView : UserControl
    {
        public TeachView()
        {
            InitializeComponent();
        }

        private void root_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.DataContext is TeachViewModel vm == false) return;
            vm.SelectedUnitTeachingOnChanged();
        }
    }
}
