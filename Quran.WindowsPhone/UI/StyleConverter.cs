using System;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml;

namespace Quran.WindowsPhone.UI
{
    public class StyleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
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

            if (Application.Current.Resources.ContainsKey(styleProperty))
            {
                return (Style)Application.Current.Resources[styleProperty];
            } else
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
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
