using System;
using System.Globalization;
using Windows.UI.Xaml.Data;

namespace Quran.WindowsPhone.UI
{
    public class TextFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (targetType != typeof(string))
            {
                throw new InvalidOperationException("The target must be a string");
            }

            if (parameter == null)
                return value;

            return string.Format(CultureInfo.InvariantCulture, parameter.ToString(), value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
