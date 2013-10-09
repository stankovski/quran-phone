using System;
using System.Globalization;
using System.Windows.Data;
using QuranPhone.Utils;

namespace QuranPhone.UI
{
    public class SettingsRetrieverConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return parameter == null ? null : SettingsUtils.Get<object>(parameter.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return parameter == null ? null : SettingsUtils.Get<object>(parameter.ToString());
        }
    }
}