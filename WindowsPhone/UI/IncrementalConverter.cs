using System;
using System.Globalization;
using System.Windows.Data;

namespace QuranPhone.UI
{
    public class IncrementalConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(double))
            {
                throw new InvalidOperationException("The target must be a double");
            }

            double valueDouble = 0;
            if (value == null || !double.TryParse(value.ToString(), out valueDouble))
                return value;

            double paramDouble = 0;
            if (parameter != null && double.TryParse(parameter.ToString(), out paramDouble))
                return valueDouble + paramDouble;
            else
                return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(double))
            {
                throw new InvalidOperationException("The target must be a double");
            }

            double valueDouble = 0;
            if (value == null || !double.TryParse(value.ToString(), out valueDouble))
                return value;

            double paramDouble = 0;
            if (parameter != null && double.TryParse(parameter.ToString(), out paramDouble))
                return valueDouble - paramDouble;
            else
                return value;
        }
    }
}
