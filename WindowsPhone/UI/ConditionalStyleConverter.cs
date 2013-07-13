using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace QuranPhone.UI
{
    public class ConditionalStyleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
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
                (condition.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                condition.Equals("visible", StringComparison.OrdinalIgnoreCase)))
            {
                return (Style)Application.Current.TryFindResource(styleName);
            }
            else
            {
                return null;
            }
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
