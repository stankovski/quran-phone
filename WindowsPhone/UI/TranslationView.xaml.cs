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
            stackPanel.Children.Clear();
            if (string.IsNullOrEmpty(formattedText))
                return;
            
            foreach (var line in formattedText.Split('\r', '\n'))
            {
                var run = getTextBlock(line, SettingsUtils.Get<int>(Constants.PREF_TRANSLATION_TEXT_SIZE));
                stackPanel.Children.Add(run);
            }
        }

        private TextBlock getTextBlock(string line, double fontSize)
        {
            bool isHeader = line.StartsWith("h:");
            bool isBold = line.StartsWith("b:");
            bool isArabic = line.StartsWith("a:");
            var textBlock = new TextBlock() { Text = line, FontSize = fontSize, Margin = new Thickness(0),
                Foreground = new SolidColorBrush(Colors.Black), TextWrapping = TextWrapping.Wrap};
            if (isHeader)
            {
                textBlock.Text = line.Substring(2);
                textBlock.FontSize = 50;
                textBlock.Foreground = new SolidColorBrush(new Color { A = 0xFF, R = 0x49, G = 0xA4, B = 0xC5 });
            }
            else if (isBold)
            {
                textBlock.Text = line.Substring(2);
                textBlock.FontWeight = FontWeights.Bold;
            }
            else if (isArabic)
            {
                textBlock.Text = line.Substring(2);
                textBlock.FontSize = fontSize * Constants.ARABIC_FONT_SCALE_RELATIVE_TO_TRANSLATION;
                textBlock.FontFamily = new FontFamily("/Assets/UthmanicHafs.otf#KFGQPC Uthmanic Script HAFS");
                textBlock.FlowDirection = FlowDirection.RightToLeft;
            }
            return textBlock;
        }

        public void Dispose()
        {
            DataContext = null;
        }
    }
}
