using System;
using System.Reflection;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI;

namespace Quran.WindowsPhone.UI
{
    public class StringBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (targetType != typeof(Brush))
            {
                throw new InvalidOperationException("The target must be a Brush");
            }

            if (value == null)
                return null;

            var stringValue = value.ToString();
            if (stringValue.StartsWith("Resource:") && App.Current.Resources.ContainsKey(stringValue.Substring("Resource:".Length)))
            {
                return (LinearGradientBrush)App.Current.Resources["Resource:".Length)];
            }                
            else
            {
                return new SolidColorBrush(GetColorFromString(stringValue));
            }
        }

        public Color GetColorFromString(string colorString)
        {
            Type colorType = (typeof (Colors));
            if (colorType.GetProperty(colorString) != null)
            {
                foreach (var color in typeof(Colors).GetRuntimeProperties())
                {
                    if (color.Name.Equals(colorString, StringComparison.OrdinalIgnoreCase))
                    {
                        return (Color)color.GetValue(null);
                    }
                }
            }
            return Colors.Black;
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
