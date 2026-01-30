using System;
using System.Windows.Controls;
using EQX.TemplateProject.MVVM.ViewModels;
using EQX.Core.Process;
using EQX.TemplateProject.Defines;

namespace EQX.TemplateProject.MVVM.Views
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
