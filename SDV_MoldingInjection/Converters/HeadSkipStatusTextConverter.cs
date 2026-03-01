using System;
using System.Globalization;
using System.Windows.Data;

namespace SDV_MoldingInjection.Converters
{
    public class HeadSkipStatusTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isSkipped)
            {
                return isSkipped ?"UNUSE" : "USE";
            }

            return "USE";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
