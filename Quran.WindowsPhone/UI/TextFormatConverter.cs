using System;
using System.Globalization;
using System.Windows.Data;

namespace Quran.WindowsPhone.UI
{
    public class TextFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(string))
            {
                throw new InvalidOperationException("The target must be a string");
            }

            if (parameter == null)
                return value;

            return string.Format(CultureInfo.InvariantCulture, parameter.ToString(), value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
