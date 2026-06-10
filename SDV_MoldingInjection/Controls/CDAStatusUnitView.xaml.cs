using System.Windows;
using System.Windows.Controls;

namespace SDV_MoldingInjection.Controls
{
    /// <summary>
    /// Interaction logic for CDAStatusUnitView.xaml
    /// </summary>
    public partial class CDAStatusUnitView : UserControl
    {
        public CDAStatusUnitView()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public string CDAName
        {
            get { return (string)GetValue(CDANameProperty); }
            set { SetValue(CDANameProperty, value); }
        }
        // Using a DependencyProperty as the backing store for CDAName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CDANameProperty =
            DependencyProperty.Register(nameof(CDAName), typeof(string), typeof(CDAStatusUnitView), new PropertyMetadata("CDA"));


        public double CDAValue
        {
            get { return (double)GetValue(CDAValueProperty); }
            set { SetValue(CDAValueProperty, value); }
        }
        // Using a DependencyProperty as the backing store for CDAValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CDAValueProperty =
            DependencyProperty.Register(nameof(CDAValue), typeof(double), typeof(CDAStatusUnitView), new PropertyMetadata(0.0));

        public bool IsThreshold
        {
            get { return (bool)GetValue(IsThresholdProperty); }
            set { SetValue(IsThresholdProperty, value); }
        }
        // Using a DependencyProperty as the backing store for IsThreshold.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsThresholdProperty =
            DependencyProperty.Register(nameof(IsThreshold), typeof(bool), typeof(CDAStatusUnitView), new PropertyMetadata(false));
    }
}
