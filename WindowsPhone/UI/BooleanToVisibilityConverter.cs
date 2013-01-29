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
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(Visibility))
            {
                throw new InvalidOperationException("The target must be a Visibility");
            }

            bool isInverse = false;

            if (value == null)
                return null;

            if (parameter != null && parameter.ToString().Equals("inverse", StringComparison.InvariantCultureIgnoreCase))
                isInverse = true;

            var boolProperty = value as bool?;

            if (boolProperty == true)
                return (isInverse ? Visibility.Collapsed : Visibility.Visible);
            else
                return (isInverse ? Visibility.Visible : Visibility.Collapsed);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(bool))
            {
                throw new InvalidOperationException("The target must be a bool");
            }

            bool isInverse = false;

            if (value == null)
                return null;

            if (parameter != null && parameter.ToString().Equals("inverse", StringComparison.InvariantCultureIgnoreCase))
                isInverse = true;

            var visibilityProperty = value as Visibility?;
            if (visibilityProperty == null)
                return null;

            if (visibilityProperty == Visibility.Visible)
                return (isInverse ? false: true);
            else
                return (isInverse ? true : false);
        }
    }
}
