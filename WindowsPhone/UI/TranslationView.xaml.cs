using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using QuranPhone.Data;
using QuranPhone.Utils;

namespace QuranPhone.UI
{
    public partial class TranslationView : UserControl, IDisposable
    {
        public TranslationView()
        {
            InitializeComponent();
        }

        public string FormattedText
        {
            get { return (string)GetValue(FormattedTextProperty); }
            set { SetValue(FormattedTextProperty, value); }
        }

        public static readonly DependencyProperty FormattedTextProperty = DependencyProperty.Register("FormattedText",
            typeof(string), typeof(TranslationView), new PropertyMetadata(
            new PropertyChangedCallback(changeFormattedText)));

        private static void changeFormattedText(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            (source as TranslationView).setFormattedText(e.NewValue as string);
        }

        private void setFormattedText(string formattedText)
        {
            translationTextBlock.Inlines.Clear();
            if (string.IsNullOrEmpty(formattedText))
                return;

            var inlines = new List<Inline>();
            foreach (var line in formattedText.Split('\r', '\n'))
            {
                var run = getRun(line, SettingsUtils.Get<int>(Constants.PREF_TRANSLATION_TEXT_SIZE));
                inlines.Add(run);
                inlines.Add(new LineBreak());
            }
            inlines.ForEach(il => translationTextBlock.Inlines.Add(il));
        }

        private Run getRun(string line, double fontSize)
        {
            bool isHeader = line.StartsWith("h:");
            bool isBold = line.StartsWith("b:");
            bool isArabic = line.StartsWith("a:");
            Run run = new Run() {Text = line, FontSize = fontSize};
            if (isHeader)
            {
                run.Text = line.Substring(2);
                run.FontSize = 50;
                run.Foreground = new SolidColorBrush(new Color { A = 0xFF, R = 0x49, G = 0xA4, B = 0xC5 });
            }
            else if (isBold)
            {
                run.Text = line.Substring(2);
                run.FontWeight = FontWeights.Bold;
            }
            else if (isArabic)
            {
                run.Text = line.Substring(2);
                run.FontSize = fontSize * Constants.ARABIC_FONT_SCALE_RELATIVE_TO_TRANSLATION;
                run.FontFamily = new FontFamily("/Assets/UthmanicHafs.otf#KFGQPC Uthmanic Script HAFS");
                run.FlowDirection = FlowDirection.RightToLeft;
            }
            return run;
        }

        public void Dispose()
        {
            DataContext = null;
        }
    }
}
