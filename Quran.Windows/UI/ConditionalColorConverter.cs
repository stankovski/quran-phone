using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Quran.Windows.UI
{
    public class ConditionalColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            if (targetType != typeof(Brush))
            {
                throw new InvalidOperationException("The target must be a Brush");
            }

            if (value == null)
                return null;

            var inverse = value as bool?;

            var color = parameter as string;

            if (inverse != null)
            {
                return new SolidColorBrush(HexStringToColor(color, inverse.Value));
            }
            else
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new NotImplementedException();
        }

        public static Color HexStringToColor(string hexColor, bool inverse)
        {
            string hc = ExtractHexDigits(hexColor);
            if (hc.Length != 8)
            {
                // you can choose whether to throw an exception
                //throw new ArgumentException("hexColor is not exactly 6 digits.");
                return Color.FromArgb(0,0,0,0);
            }
            string a = hc.Substring(0, 2);
            string r = hc.Substring(2, 2);
            string g = hc.Substring(4, 2);
            string b = hc.Substring(6, 2);
            Color color = Color.FromArgb(0, 0, 0, 0);
            try
            {
                byte ai = byte.Parse(a, NumberStyles.HexNumber);
                byte ri = byte.Parse(r, NumberStyles.HexNumber);
                byte gi = byte.Parse(g, NumberStyles.HexNumber);
                byte bi = byte.Parse(b, NumberStyles.HexNumber);
                color = Color.FromArgb(ai, ri, gi, bi);
                if (inverse)
                {
                    color = Color.FromArgb(ai, (byte)(255-ri), (byte)(255-gi), (byte)(255-bi));
                }
            }
            catch
            {
                // you can choose whether to throw an exception
                //throw new ArgumentException("Conversion failed.");
                return Color.FromArgb(0, 0, 0, 0);
            }
            return color;
        }

        public static string ExtractHexDigits(string input)
        {
            // remove any characters that are not digits (like #)
            var isHexDigit
                = new Regex("[abcdefABCDEF\\d]+", RegexOptions.Compiled);
            string newnum = "";
            foreach (char c in input)
            {
                if (isHexDigit.IsMatch(c.ToString()))
                {
                    newnum += c.ToString();
                }
            }
            return newnum;
        }
    }
}
