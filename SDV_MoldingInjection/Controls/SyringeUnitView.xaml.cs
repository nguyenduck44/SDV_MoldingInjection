using EQX.UI.Controls;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.Recipe;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace SDV_MoldingInjection.Controls
{
    /// <summary>
    /// Interaction logic for SyringeUnitView.xaml
    /// </summary>
    public partial class SyringeUnitView : UserControl
    {
        public SyringeUnitView()
        {
            InitializeComponent();
        }

        private void Border_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (this.DataContext is SyringeAmountStatus syringe == false) return;

            var result = MessageBoxEx.ShowDialog("Do you want to Reset Syringe Amount?", true, "Confirm");

            if (result == true)
            {
                syringe.Reset();
            }
        }
    }
}
