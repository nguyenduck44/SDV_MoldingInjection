using EQX.UI.Controls;
using SDV_MoldingInjection.Defines.Devices;
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
        private readonly DispatcherTimer _timer;

        public SyringeUnitView()
        {
            InitializeComponent();

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        public SPDHeadRecipe Recipe
        {
            get { return (SPDHeadRecipe)GetValue(RecipeProperty); }
            set { SetValue(RecipeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RecipeProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RecipeProperty =
            DependencyProperty.Register("Recipe", typeof(SPDHeadRecipe), typeof(SyringeUnitView), new PropertyMetadata(null));

        public SyringeAmountStatus SyringeAmountStatus
        {
            get { return (SyringeAmountStatus)GetValue(SyringeAmountStatusProperty); }
            set { SetValue(SyringeAmountStatusProperty, value); }
        }

        public static readonly DependencyProperty SyringeAmountStatusProperty =
        DependencyProperty.Register("SyringeAmountStatus", typeof(SyringeAmountStatus), typeof(SyringeUnitView), new PropertyMetadata(null));

        public TimeSpan ElapsedTime
        {
            get { return (TimeSpan)GetValue(ElapsedTimeProperty); }
            set { SetValue(ElapsedTimeProperty, value); }
        }

        public static readonly DependencyProperty ElapsedTimeProperty =
            DependencyProperty.Register("ElapsedTime", typeof(TimeSpan), typeof(SyringeUnitView), new PropertyMetadata(TimeSpan.Zero));

        private void Border_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (SyringeAmountStatus == null) return;

            var result = MessageBoxEx.ShowDialog("Do you want to Reset Syringe Amount?", true, "Confirm");

            if (result == true)
            {
                SyringeAmountStatus.Reset();
            }
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (SyringeAmountStatus != null)
            {
                ElapsedTime = DateTime.Now - SyringeAmountStatus.ResetTime;
            }
            else
            {
                ElapsedTime = TimeSpan.Zero;
            }
        }

    }
}
