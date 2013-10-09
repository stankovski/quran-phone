using System;
using System.Globalization;
using System.Windows.Data;
using QuranPhone.Data;
using QuranPhone.Utils;

namespace QuranPhone.UI
{
    public class ArabicFontSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof (double))
            {
                throw new InvalidOperationException("The target must be double");
            }

            if (value != null && value.ToString() == "TranslationViewHeader")
            {
                return 50;
            }
            if (value != null && value.ToString() == "ArabicText")
            {
                return (SettingsUtils.Get<int>(Constants.PrefTranslationTextSize)*
                        Constants.ArabicFontScaleRelativeToTranslation);
            }
            return SettingsUtils.Get<int>(Constants.PrefTranslationTextSize);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}