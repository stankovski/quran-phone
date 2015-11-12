using Quran.Core.ViewModels;
using Windows.UI.Xaml.Controls;

namespace Quran.WindowsPhone.UI
{
    public partial class PageLink : UserControl
    {
        public ItemViewModel ViewModel{ get; set; }

        public PageLink()
        {
            InitializeComponent();

            DataContextChanged += (s,e)=> { ViewModel = DataContext as ItemViewModel; };
        }
    }
}
