using System;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Quran.WindowsPhone.UI;

namespace Quran.WindowsPhone.UI
{
    public class StringBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(Brush))
            {
                throw new InvalidOperationException("The target must be a Brush");
            }

            if (value == null)
                return null;

            var stringValue = value.ToString();
            if (stringValue.StartsWith("Resource:"))
                return
                    (LinearGradientBrush) Application.Current.TryFindResource(stringValue.Substring("Resource:".Length));
            else
            {
                return new SolidColorBrush(GetColorFromString(stringValue));
            }
        }

        public Color GetColorFromString(string colorString)
        {
            Type colorType = (typeof (System.Windows.Media.Colors));
            if (colorType.GetProperty(colorString) != null)
            {
                object o = colorType.InvokeMember(colorString, BindingFlags.GetProperty, null, null, null);
                if (o != null)

                {
                    return (Color) o;
                }
            }
            return Colors.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
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
