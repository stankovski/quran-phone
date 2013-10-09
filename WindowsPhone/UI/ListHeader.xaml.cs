using System.Windows;
using System.Windows.Controls;

namespace QuranPhone.UI
{
    public partial class ListHeader : UserControl
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof (string),
            typeof (ListHeader), new PropertyMetadata(changeSource));

        public ListHeader()
        {
            InitializeComponent();
        }

        public string Text
        {
            get { return (string) GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        private static void changeSource(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            (source as ListHeader).updateText(e.NewValue as string);
        }

        private void updateText(string text)
        {
            textBlock.Text = text;
        }
    }
}