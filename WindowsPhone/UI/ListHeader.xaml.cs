using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace QuranPhone.UI
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
