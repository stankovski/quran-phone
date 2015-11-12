using System.Windows;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Quran.WindowsPhone.UI
{
    public partial class ListHeader : UserControl
    {
        public ListHeader()
        {
            InitializeComponent();
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text",
            typeof(string), typeof(ListHeader), new PropertyMetadata(
            new PropertyChangedCallback(changeSource)));

        private static void changeSource(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            (source as ListHeader).updateText(e.NewValue as string);
        }

        private void updateText(string text)
        {
            this.textBlock.Text = text; 
        }
    }
}
