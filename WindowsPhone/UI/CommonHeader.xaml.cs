using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using QuranPhone.Data;
using QuranPhone.Utils;

namespace QuranPhone.UI
{
    public partial class CommonHeader : UserControl
    {
        public CommonHeader()
        {
            InitializeComponent();
        }

        private void OpenLastPage(object sender, RoutedEventArgs e)
        {
            var lastPage = SettingsUtils.Get<int>(Constants.PrefLastPage);
            if (lastPage == 0)
            {
                lastPage = 1;
            }
            if (NavigationRequest != null)
            {
                NavigationRequest(this,
                    new NavigationEventArgs(this, new Uri("/DetailsPage.xaml?page=" + lastPage, UriKind.Relative)));
            }
        }

        public event NavigatedEventHandler NavigationRequest;
    }
}