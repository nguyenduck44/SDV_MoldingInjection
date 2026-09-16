using EQX.UI.Controls;
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

namespace SDV_MoldingInjection.Resource.Controls
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class SingleAxisSpeedConfig : UserControl
    {
        public SingleAxisSpeedConfig()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public string AxisName
        {
            get { return (string)GetValue(AxisNameProperty); }
            set { SetValue(AxisNameProperty, value); }
        }
        // Using a DependencyProperty as the backing store for AxisName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AxisNameProperty =
            DependencyProperty.Register(nameof(AxisName), typeof(string), typeof(SingleAxisSpeedConfig), new PropertyMetadata(string.Empty));

        public double Speed
        {
            get { return (double)GetValue(SpeedProperty); }
            set { SetValue(SpeedProperty, value); }
        }
        // Using a DependencyProperty as the backing store for Speed.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SpeedProperty =
            DependencyProperty.Register(nameof(Speed), typeof(double), typeof(SingleAxisSpeedConfig), new PropertyMetadata(0.0));

        public double Accel
        {
            get { return (double)GetValue(AccelProperty); }
            set { SetValue(AccelProperty, value); }
        }
        // Using a DependencyProperty as the backing store for Accel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AccelProperty =
            DependencyProperty.Register(nameof(Accel), typeof(double), typeof(SingleAxisSpeedConfig), new PropertyMetadata(0.0));

        public double Decel
        {
            get { return (double)GetValue(DecelProperty); }
            set { SetValue(DecelProperty, value); }
        }
        // Using a DependencyProperty as the backing store for Decel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DecelProperty =
            DependencyProperty.Register(nameof(Decel), typeof(double), typeof(SingleAxisSpeedConfig), new PropertyMetadata(0.0));

        private void TextBox_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBox textBox == false) return;

            DataEditor dataEditor = new DataEditor(Convert.ToDouble(textBox.Text), null);
            if (dataEditor.ShowDialog() == true)
            {
                textBox.Text = dataEditor.NewValue.ToString();
            }
        }
    }
}
