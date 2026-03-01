using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SDV_MoldingInjection.Converters
{
    public class HeadSkipStatusBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isChecked = value is bool b && !b;
            string role = parameter?.ToString() ?? "Background";

            return role switch
            {
                "Foreground" => isChecked ? Brushes.Black : Brushes.White,
                "Border" => isChecked ? Brushes.Lime : Brushes.Gray,
                _ => isChecked ? Brushes.Lime : Brushes.Gray
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}