using System;
using System.Globalization;
using Quran.Core.Data;
using Quran.Core.Utils;
using Windows.UI.Xaml.Data;

namespace Quran.WindowsPhone.UI
{
    public class ArabicFontSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (targetType != typeof(double))
            {
                throw new InvalidOperationException("The target must be double");
            }

            if (value != null && value.ToString() == "TranslationViewHeader")
                return 50;
            else if (value != null && value.ToString() == "ArabicText")
                return (SettingsUtils.Get<int>(Constants.PREF_TRANSLATION_TEXT_SIZE) *
                        Constants.ARABIC_FONT_SCALE_RELATIVE_TO_TRANSLATION);
            else
                return SettingsUtils.Get<int>(Constants.PREF_TRANSLATION_TEXT_SIZE);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
