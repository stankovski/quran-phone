using Quran.Core.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Quran.Windows.UI
{
    public partial class PageLink : UserControl
    {
        public ItemViewModel ViewModel{ get; set; }

        public PageLink()
        {
            InitializeComponent();            
        }

        public static readonly DependencyProperty GlyphProperty = DependencyProperty.Register("Glyph", typeof(string), typeof(PageLink), null);

        public string Glyph
        {
            get { return GetValue(GlyphProperty) as string; }
            set { SetValue(GlyphProperty, value); }
        }

        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register("Label", typeof(string), typeof(PageLink), null);

        public string Label
        {
            get { return GetValue(LabelProperty) as string; }
            set { SetValue(LabelProperty, value); }
        }
    }
}
