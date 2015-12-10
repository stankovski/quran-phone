using System;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Quran.Windows.UI;

namespace Quran.Windows.UI
{
    public class AudioPlayerOffsetConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(double))
            {
                throw new InvalidOperationException("The target must be a double");
            }

            if (value == null)
                return null;

            var doubleValue = (double)value;

            return (doubleValue*-0.60);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
