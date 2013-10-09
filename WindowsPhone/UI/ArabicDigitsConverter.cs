using System;
using System.Globalization;
using System.Windows.Data;

namespace QuranPhone.UI
{
    public class ArabicDigitsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            switch (CultureInfo.CurrentCulture.TwoLetterISOLanguageName)
            {
                case "ar":
                    return
                        value.ToString()
                            .Replace("0", "٠")
                            .Replace("1", "١")
                            .Replace("2", "٢")
                            .Replace("3", "٣")
                            .Replace("4", "٤")
                            .Replace("5", "٥")
                            .Replace("6", "٦")
                            .Replace("7", "٧")
                            .Replace("8", "٨")
                            .Replace("9", "٩");
                case "fa":
                    return
                        value.ToString()
                            .Replace("0", "۰")
                            .Replace("1", "۱")
                            .Replace("2", "۲")
                            .Replace("3", "۳")
                            .Replace("4", "۴")
                            .Replace("5", "۵")
                            .Replace("6", "۶")
                            .Replace("7", "۷")
                            .Replace("8", "۸")
                            .Replace("9", "۹");
                default:
                    return value.ToString();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}