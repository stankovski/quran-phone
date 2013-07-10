using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using QuranPhone.Data;
using QuranPhone.Utils;

namespace QuranPhone.UI
{
    public class ArabicFontSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(double))
            {
                throw new InvalidOperationException("The target must be double");
            }

            if (value != null && value.ToString() == "ArabicText")
                return (SettingsUtils.Get<int>(Constants.PREF_TRANSLATION_TEXT_SIZE)* Constants.ARABIC_FONT_SCALE_RELATIVE_TO_TRANSLATION);
            else
                return SettingsUtils.Get<int>(Constants.PREF_TRANSLATION_TEXT_SIZE);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
