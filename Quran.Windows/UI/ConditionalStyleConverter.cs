using System;
using Quran.Windows.UI;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Quran.Windows.UI
{
    public class ConditionalStyleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (targetType != typeof(Style))
            {
                throw new InvalidOperationException("The target must be a Style");
            }

            if (value == null)
                return null;

            var condition = value.ToString();

            var styleName = parameter as string;
            
            if (condition != null &&
                App.Current.Resources.ContainsKey(styleName) &&
                (condition.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                condition.Equals("visible", StringComparison.OrdinalIgnoreCase)))
            {
                return (Style)App.Current.Resources[styleName];
            }
            else
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
