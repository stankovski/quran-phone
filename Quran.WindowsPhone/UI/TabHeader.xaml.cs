using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Quran.WindowsPhone.UI
{
    public sealed partial class TabHeader : UserControl
    {
        public TabHeader()
        {
            this.InitializeComponent();
            this.DataContext = this;
        }

        public static readonly DependencyProperty GlyphProperty = DependencyProperty.Register("Glyph", typeof(string), typeof(TabHeader), null);

        public string Glyph
        {
            get { return GetValue(GlyphProperty) as string; }
            set { SetValue(GlyphProperty, value); }
        }

        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register("Label", typeof(string), typeof(TabHeader), null);

        public string Label
        {
            get { return GetValue(LabelProperty) as string; }
            set { SetValue(LabelProperty, value); }
        }
    }
}
