using System;
using Windows.UI.Xaml.Controls;
using Quran.Core.Utils;
using Quran.Core.Data;
using Windows.UI.Xaml;
using Quran.WindowsPhone.Views;

namespace Quran.WindowsPhone.UI
{
    public sealed partial class CommonHeader : UserControl
    {
        public CommonHeader()
        {
            this.InitializeComponent();
        }

        private void OpenLastPage(object sender, RoutedEventArgs e)
        {
            var lastPage = SettingsUtils.Get<int>(Constants.PREF_LAST_PAGE);
            if (lastPage == 0)
                lastPage = 1;
            if (NavigationRequest != null)
                NavigationRequest(this, typeof(MainView), new object[] { lastPage });
        }

        public delegate void NavigationRequestHandler(object source, Type viewType, object[] parameters);
        public event NavigationRequestHandler NavigationRequest;
    }
}
