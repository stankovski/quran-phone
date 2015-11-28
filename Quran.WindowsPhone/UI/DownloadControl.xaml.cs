using Quran.Core.ViewModels;
using Windows.UI.Xaml.Controls;

namespace Quran.WindowsPhone.UI
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
