using SDV_MoldingInjection.Defines;
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

namespace SDV_MoldingInjection.Controls
{
    /// <summary>
    /// Interaction logic for CarrierJigStatusView.xaml
    /// </summary>
    public partial class CarrierJigStatusView : UserControl
    {
        public CarrierJigStatusView()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is CarrierJigStatus carrierJigStatus)
            {
                carrierJigStatus.HeadSkip = !carrierJigStatus.HeadSkip;
            }
        }
    }
}
