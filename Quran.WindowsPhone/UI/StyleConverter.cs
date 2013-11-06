using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using QuranPhone.UI;

namespace Quran.WindowsPhone.UI
{
    public class StyleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(Style))
            {
                throw new InvalidOperationException("The target must be a Style");
            }

            if (value == null)
                return null;

            var styleProperty = value as string;
            var paramProperty = parameter as string;
            if (paramProperty != null)
                styleProperty = styleProperty + paramProperty;

            Style newStyle = (Style)Application.Current.TryFindResource(styleProperty);
            return newStyle;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(string))
            {
                throw new InvalidOperationException("The target must be a String");
            }

            if (value == null)
                return null;

            var styleProperty = parameter as Style;
            if (styleProperty == null)
                return null;
            else
                return styleProperty.ToString();
        }
    }
}
