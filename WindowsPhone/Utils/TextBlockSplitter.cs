using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace QuranPhone.Utils
{
    public class TextBlockSplitter
    {
        private const double MaxHeight = 2048;
        private static TextBlockSplitter _instance;
        private readonly TextBlock _measureBlock;

        private TextBlockSplitter()
        {
            _measureBlock = GenerateTextBlock();
        }

        public FontFamily FontFamily { get; set; }

        public static TextBlockSplitter Instance
        {
            get { return _instance ?? (_instance = new TextBlockSplitter()); }
        }

        private TextBlock GenerateTextBlock()
        {
            var textBlock = new TextBlock();
            textBlock.TextWrapping = TextWrapping.Wrap;
            textBlock.Margin = new Thickness(10);
            return textBlock;
        }

        public IList<string> Split(string value, double fontSize, FontWeight fontWeight)
        {
            var parsedText = new List<string>();
            var reader = new StringReader(value);
            _measureBlock.FontSize = fontSize;
            _measureBlock.FontWeight = fontWeight;
            _measureBlock.Width = QuranScreenInfo.Instance.Width;

            int maxTextCount = GetMaxTextSize();

            if (value.Length < maxTextCount)
            {
                parsedText.Add(value);
            }
            else
            {
                while (reader.Peek() > 0)
                {
                    string line = reader.ReadLine();
                    parsedText.AddRange(ParseLine(line, maxTextCount));
                }
            }
            return parsedText;
        }

        private IList<string> ParseLine(string line, int maxTextCount)
        {
            int maxLineCount = GetMaxLineCount();
            string tempLine = line;
            var parsedText = new List<string>();

            try
            {
                while (tempLine.Trim().Length > 0)
                {
                    int charactersFitted = GetCharactersThatFit(tempLine, maxTextCount);
                    parsedText.Add(tempLine.Substring(0, charactersFitted).Trim());
                    tempLine = tempLine.Substring(charactersFitted, tempLine.Length - (charactersFitted));
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            return parsedText;
        }

        private int GetCharactersThatFit(string text, int maxTextCount)
        {
            int maxLineLength = maxTextCount > text.Length ? text.Length : maxTextCount;
            for (int i = maxLineLength - 1; i > 1; i--)
            {
                if (text[i] == ' ')
                {
                    double nHeight = MeasureString(text.Substring(0, i - 1)).Height;
                    if (nHeight <= MaxHeight)
                    {
                        return i;
                    }
                }
            }
            return maxLineLength;
        }

        private Size MeasureString(string text)
        {
            _measureBlock.Text = text;
            return new Size(_measureBlock.ActualWidth, _measureBlock.ActualHeight);
        }

        private int GetMaxTextSize()
        {
            // Get average char size
            Size size = MeasureText(" ");
            // Get number of char that fit in the line
            var charLineCount = (int) (_measureBlock.Width/size.Width);
            // Get line count
            var lineCount = (int) (MaxHeight/size.Height);

            return charLineCount*lineCount/2;
        }

        private int GetMaxLineCount()
        {
            Size size = MeasureText(" ");
            // Get number of char that fit in the line
            var charLineCount = (int) (_measureBlock.Width/size.Width);
            // Get line count
            int lineCount = (int) (MaxHeight/size.Height) - 5;

            return lineCount;
        }

        private Size MeasureText(string value)
        {
            _measureBlock.Text = value;
            return new Size(_measureBlock.ActualWidth, _measureBlock.ActualHeight);
        }
    }
}