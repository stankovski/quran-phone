using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using QuranPhone.Utils;
using QuranPhone.Data;

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
            var lastPage = SettingsUtils.Get<int>(Constants.PREF_LAST_PAGE);
            if (lastPage == 0)
                lastPage = 1;
            if (NavigationRequest != null)
                NavigationRequest(this, new NavigationEventArgs(this, new Uri("/DetailsPage.xaml?page=" + lastPage, UriKind.Relative)));
        }

        public event NavigatedEventHandler NavigationRequest;
    }
}
