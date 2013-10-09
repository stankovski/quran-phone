using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace QuranPhone.UI
{
    public class StyleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof (Style))
            {
                throw new InvalidOperationException("The target must be a Style");
            }

            if (value == null)
            {
                return null;
            }

            var styleProperty = value as string;
            var paramProperty = parameter as string;
            if (paramProperty != null)
            {
                styleProperty = styleProperty + paramProperty;
            }

            return Application.Current.Resources[styleProperty];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof (string))
            {
                throw new InvalidOperationException("The target must be a String");
            }

            if (value == null)
            {
                return null;
            }

            var styleProperty = parameter as Style;
            return styleProperty == null ? null : styleProperty.ToString();
        }
    }
}