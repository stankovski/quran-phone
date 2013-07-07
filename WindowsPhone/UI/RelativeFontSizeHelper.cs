using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace QuranPhone.UI
{
    public sealed class RelativeFontSizeHelper
    {
        public static readonly DependencyProperty RelativeFontSizeProperty =
            DependencyProperty.RegisterAttached("RelativeFontSize",
                                                typeof(double),
                                                typeof(RelativeFontSizeHelper),
                                                new PropertyMetadata((double)0, RelativeFontSizeChanged));

        public static double GetRelativeFontSize(TextBlock textBlock)
        {
            if (textBlock == null)
                throw new ArgumentNullException("textBlock");

            return (double)textBlock.GetValue(RelativeFontSizeProperty);
        }

        public static void SetRelativeFontSize(TextBlock textBlock, double value)
        {
            if (textBlock == null)
                throw new ArgumentNullException("textBlock");

            textBlock.SetValue(RelativeFontSizeProperty, value);
        }

        private static void RelativeFontSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TextBlock t = d as TextBlock;

            if (t == null || e.Property != RelativeFontSizeProperty || (double)e.NewValue == 0.0)
                return;

            t.FontSize = Math.Max((t.FontSize * (double)e.NewValue), 0);
        }
    }

}
