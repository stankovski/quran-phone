using System;
using System.Windows;
using Windows.UI.Xaml.Controls;
using System.Windows.Navigation;
using Quran.Core.Utils;
using Quran.Core.Data;

namespace Quran.WindowsPhone.UI
{
    public partial class CommonHeader : UserControl
    {
        public CommonHeader()
        {
            InitializeComponent();
        }

        private void OpenLastPage(object sender, RoutedEventArgs e)
        {
            var lastPage = SettingsUtils.Get<int>(Constants.PREF_LAST_PAGE);
            if (lastPage == 0)
                lastPage = 1;
            if (NavigationRequest != null)
                NavigationRequest(this, new NavigationEventArgs(this, new Uri("/Views/DetailsView.xaml?page=" + lastPage, UriKind.Relative)));
        }

        public event NavigatedEventHandler NavigationRequest;
    }
}
