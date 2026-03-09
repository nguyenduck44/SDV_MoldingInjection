using System;
using System.Globalization;
using System.Windows.Data;

namespace SDV_MoldingInjection.Converters
{
    public class TimeSpanToTotalHoursStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not TimeSpan ts || ts < TimeSpan.Zero)
                return "0:00:00";

            int hours = (int)ts.TotalHours;
            int mins = ts.Minutes;
            int secs = ts.Seconds;
            return $"{hours}:{mins:D2}:{secs:D2}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => Binding.DoNothing;
    }
}
