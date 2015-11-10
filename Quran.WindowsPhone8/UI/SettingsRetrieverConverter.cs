using System;
using System.Globalization;
using System.Windows.Data;
using Quran.Core.Utils;

namespace Quran.WindowsPhone.UI
{
    public class SettingsRetrieverConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null)
                return null;
            else
                return SettingsUtils.Get<object>(parameter.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null)
                return null;
            else
                return SettingsUtils.Get<object>(parameter.ToString());
        }
    }
}
