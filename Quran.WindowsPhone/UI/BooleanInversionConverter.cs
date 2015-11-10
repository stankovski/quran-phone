using System;
using Windows.UI.Xaml.Data;

namespace Quran.WindowsPhone.UI
{
    public class BooleanInversionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (targetType != typeof(bool))
            {
                throw new InvalidOperationException("The target must be a boolean");
            }

            if (value == null)
                return null;

            var boolProperty = value as bool?;
            return !boolProperty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (targetType != typeof(bool))
            {
                throw new InvalidOperationException("The target must be a bool");
            }

            if (value == null)
                return null;

            var boolProperty = value as bool?;
            return !boolProperty;
        }
    }
}
