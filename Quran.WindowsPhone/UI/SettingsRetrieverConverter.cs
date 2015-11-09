using System;
using System.Globalization;
using Quran.Core.Utils;
using Windows.UI.Xaml.Data;

namespace Quran.WindowsPhone.UI
{
    public class SettingsRetrieverConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (parameter == null)
                return null;
            else
                return SettingsUtils.Get<object>(parameter.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (parameter == null)
                return null;
            else
                return SettingsUtils.Get<object>(parameter.ToString());
        }
    }
}
