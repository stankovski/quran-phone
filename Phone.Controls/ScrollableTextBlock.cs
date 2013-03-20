using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Text;

//Source: http://blogs.msdn.com/b/priozersk/archive/2010/09/08/creating-scrollable-textblock-for-wp7.aspx
namespace Phone.Controls
{
    public class ScrollableTextBlock : Control, IDisposable
    {
        private StackPanel stackPanel;
        private Dictionary<LineTypes, TextBlock> templateTextBlockCache = new Dictionary<LineTypes, TextBlock>();
        
        public ScrollableTextBlock()
        {
            // Get the style from generic.xaml
            this.DefaultStyleKey = typeof(ScrollableTextBlock);
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                "Text",
                typeof(string),
                typeof(ScrollableTextBlock),
                new PropertyMetadata("ScrollableTextBlock", OnTextPropertyChanged));

        public string Text
        {
            get
            {
                return (string)GetValue(TextProperty);
            }
            set
            {
                SetValue(TextProperty, value);
            }
        }

        private static void OnTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ScrollableTextBlock source = (ScrollableTextBlock)d;
            string value = (string)e.NewValue;
            source.ParseText(value);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.stackPanel = this.GetTemplateChild("StackPanel") as StackPanel;
            this.ParseText(this.Text);
        }


        private void ParseText(string value)
        {
            if (this.stackPanel == null || value == null)
            {
                return;
            }

            // Clear previous TextBlocks
            this.stackPanel.Children.Clear();

            foreach (var line in value.Split('\r', '\n'))
            {
                // Calculate max char count
                int maxTexCount = this.GetMaxTextSize(LineTypes.Regular);

                if (line.Length < maxTexCount)
                {
                    TextBlock textBlock = this.GetTextBlock(line);
                    this.stackPanel.Children.Add(textBlock);
                }
                else
                {
                    ParseLine(line);
                }
            }
        }

        private void ParseLine(string line)
        {
            int lineCount = 0;
            string tempLine = line;
            var sbLine = new StringBuilder();
            var lineType = GetLineType(line);
            int maxLineCount = GetMaxLineCount(lineType);
            
            while (lineCount < maxLineCount)
            {
                int charactersFitted = MeasureString(tempLine, (int)this.Width, lineType);
                string leftSide = tempLine.Substring(0, charactersFitted);
                sbLine.Append(leftSide);
                tempLine = tempLine.Substring(charactersFitted, tempLine.Length - (charactersFitted));
                lineCount++;
            }

            TextBlock textBlock = this.GetTextBlock(sbLine.ToString());
            this.stackPanel.Children.Add(textBlock);

            if (tempLine.Length > 0)
            {
                ParseLine(tempLine);
            }           
        }

        private TextBlock GetTemplateTextBlock(LineTypes lineType)
        {
            if (templateTextBlockCache.ContainsKey(lineType))
                return templateTextBlockCache[lineType];

            var textBlock = new TextBlock()
            {
                FontSize = this.FontSize,
                Margin = new Thickness(0),
                Foreground = new SolidColorBrush(Colors.Black),
                TextWrapping = TextWrapping.Wrap
            };
            switch (lineType)
            {
                case LineTypes.Header:
                    textBlock.FontSize = 50;
                    textBlock.Foreground = new SolidColorBrush(new Color { A = 0xFF, R = 0x49, G = 0xA4, B = 0xC5 });
                    break;

                case LineTypes.Bold:
                    textBlock.FontWeight = FontWeights.Bold;
                    break;

                case LineTypes.Arabic:
                    textBlock.FontSize = this.FontSize * 2;
                    textBlock.FontFamily = new FontFamily("/Assets/UthmanicHafs.otf#KFGQPC Uthmanic Script HAFS");
                    textBlock.FlowDirection = FlowDirection.RightToLeft;
                    break;
            }
            templateTextBlockCache[lineType] = textBlock;

            return textBlock;
        }

        private TextBlock GetTextBlock(string line)
        {
            var lineType = GetLineType(line);

            var textBlock = new TextBlock()
            {
                Text = line,
                FontSize = this.FontSize,
                Margin = new Thickness(0, 0, this.Margin.Right, 0),
                Foreground = new SolidColorBrush(Colors.Black),
                TextWrapping = TextWrapping.Wrap
            };
            switch (lineType)
            {
                case LineTypes.Header:
                    textBlock.Text = line.Substring(2);
                    textBlock.FontSize = 50;
                    textBlock.Foreground = new SolidColorBrush(new Color { A = 0xFF, R = 0x49, G = 0xA4, B = 0xC5 });
                    break;

                case LineTypes.Bold:
                    textBlock.Text = line.Substring(2);
                    textBlock.FontWeight = FontWeights.Bold;
                    break;

                case LineTypes.Arabic:
                    textBlock.Text = line.Substring(2);
                    textBlock.FontSize = this.FontSize * 1.8;
                    textBlock.FontFamily = new FontFamily("/Assets/UthmanicHafs.otf#KFGQPC Uthmanic Script HAFS");
                    textBlock.FlowDirection = FlowDirection.RightToLeft;
                    break;
            }
            return textBlock;
        }

        private static LineTypes GetLineType(string line)
        {
            var lineType = LineTypes.Regular;
            if (line.StartsWith("h:"))
                lineType = LineTypes.Header;
            if (line.StartsWith("b:"))
                lineType = LineTypes.Bold;
            if (line.StartsWith("a:"))
                lineType = LineTypes.Arabic;
            return lineType;
        }

        private int MeasureString(string text, int desWidth, LineTypes lineType)
        {
            
            int nWidth = 0;
            int charactersFitted = 0;

            StringBuilder sb = new StringBuilder();

            //get original size
            Size size = MeasureString(text, lineType);

            if (size.Width > desWidth)
            {
                string[] words = text.Split(' ');
                sb.Append(words[0]);

                for (int i = 1; i < words.Length; i++)
                {
                    sb.Append(" " + words[i]);
                    nWidth = (int)MeasureString(sb.ToString(), lineType).Width;

                    if (nWidth > desWidth)
                    {

                        sb.Remove(sb.Length - words[i].Length, words[i].Length);
                        break;
                    }
                }

                charactersFitted = sb.Length;
            }
            else
            {
                charactersFitted = text.Length;
            }

            return charactersFitted;
        }

        private Size MeasureString(string text, LineTypes lineType)
        {
            var template = GetTemplateTextBlock(lineType);
            template.Text = text;
            return new Size(template.ActualWidth, template.ActualHeight);
        }

        private int GetMaxTextSize(LineTypes lineType)
        {
            // Get average char size
            Size size = this.MeasureText(" ", lineType);
            // Get number of char that fit in the line
            int charLineCount = (int)(this.Width / size.Width);
            // Get line count
            int lineCount = (int)(2048 / size.Height);

            return charLineCount * lineCount / 2;
        }

        private int GetMaxLineCount(LineTypes lineType)
        {
            Size size = this.MeasureText(" ", lineType);
            // Get number of char that fit in the line
            int charLineCount = (int)(this.Width / size.Width);
            // Get line count
            int lineCount = (int)(2048 / size.Height) - 5;

            return lineCount;
        }

        //private TextBlock GetTextBlock()
        //{
        //    TextBlock textBlock = new TextBlock();
        //    textBlock.TextWrapping = TextWrapping.Wrap;
        //    textBlock.FontSize = this.FontSize;
        //    textBlock.FontFamily = this.FontFamily;
        //    // textBlock.FontStyle = this.FontStyle;
        //    textBlock.FontWeight = this.FontWeight;
        //    textBlock.Foreground = this.Foreground;
        //    textBlock.Margin = new Thickness(0, 0, MeasureText(" ").Width, 0);
        //    return textBlock;
        //}

        private Size MeasureText(string value, LineTypes lineType)
        {
            var template = GetTemplateTextBlock(lineType);
            template.Text = value;
            return new Size(template.ActualWidth, template.ActualHeight);
        }

        public void Dispose()
        {
            foreach (var textBlock in templateTextBlockCache.Values)
                textBlock.Text = null;
            templateTextBlockCache.Clear();
        }
    }
}
