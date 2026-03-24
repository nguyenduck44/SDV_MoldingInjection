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
    /// Interaction logic for ProductionDayView.xaml
    /// </summary>
    public partial class ProductionDayView : UserControl
    {
        public ProductionDayView()
        {
            InitializeComponent();
            Loaded += (_, _) => UpdateShiftView();
        }

        private void ShiftViewRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            UpdateShiftView();
        }

        private void UpdateShiftView()
        {
            if (AllShiftRadioButton == null || AllShiftGroupBox == null || DayShiftGroupBox == null || NightShiftGroupBox == null)
            {
                return;
            }

            if (AllShiftRadioButton.IsChecked == true)
            {
                AllShiftGroupBox.Visibility = Visibility.Visible;
                DayShiftGroupBox.Visibility = Visibility.Collapsed;
                NightShiftGroupBox.Visibility = Visibility.Collapsed;
                return;
            }

            if (DayShiftRadioButton.IsChecked == true)
            {
                AllShiftGroupBox.Visibility = Visibility.Collapsed;
                DayShiftGroupBox.Visibility = Visibility.Visible;
                NightShiftGroupBox.Visibility = Visibility.Collapsed;
                return;
            }

            AllShiftGroupBox.Visibility = Visibility.Collapsed;
            DayShiftGroupBox.Visibility = Visibility.Collapsed;
            NightShiftGroupBox.Visibility = Visibility.Visible;
        }
    }
}
