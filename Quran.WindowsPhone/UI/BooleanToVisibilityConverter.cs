using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Quran.WindowsPhone.UI
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (targetType != typeof(Visibility))
            {
                throw new InvalidOperationException("The target must be a Visibility");
            }

            bool isInverse = false;

            if (value == null)
                return null;

            if (parameter != null && parameter.ToString().Equals("inverse", StringComparison.OrdinalIgnoreCase))
                isInverse = true;

            var boolProperty = value as bool?;

            if (boolProperty == true)
                return (isInverse ? Visibility.Collapsed : Visibility.Visible);
            else
                return (isInverse ? Visibility.Visible : Visibility.Collapsed);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (targetType != typeof(bool))
            {
                throw new InvalidOperationException("The target must be a bool");
            }

            bool isInverse = false;

            if (value == null)
                return null;

            if (parameter != null && parameter.ToString().Equals("inverse", StringComparison.OrdinalIgnoreCase))
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
