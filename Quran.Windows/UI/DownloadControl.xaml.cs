using Quran.Core.ViewModels;
using Windows.UI.Xaml.Controls;

namespace Quran.Windows.UI
{
    public partial class DownloadControl : UserControl
    {
        public DownloadableViewModelBase ViewModel { get; set; }

        public DownloadControl()
        {
            InitializeComponent();
        }
    }
}
