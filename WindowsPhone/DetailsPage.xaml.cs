using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using QuranPhone.Resources;
using QuranPhone.ViewModels;
using QuranPhone.Utils;

namespace QuranPhone
{
    public partial class DetailsPage : PhoneApplicationPage
    {
        //QuranScreenInfo screenInfo;
        // Constructor
        public DetailsPage()
        {
            InitializeComponent();
            //screenInfo = QuranScreenInfo.GetInstance();
            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
        }

        // When page is navigated to set data context to selected item in list
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (DataContext == null)
            {
                string selectedPage = "";
                if (NavigationContext.QueryString.TryGetValue("selectedItem", out selectedPage))
                {
                    int index = int.Parse(selectedPage);
                    var viewModel = new DetailsViewModel();
                    viewModel.LoadData();
                    DataContext = viewModel;                    
                }
            }
        }

        private void Pivot_LoadingItem(object sender, PivotItemEventArgs e)
        {
            var pageModel = (PageViewModel)e.Item.DataContext;
            pageModel.ImageSource = QuranFileUtils.GetImageFromWeb(QuranFileUtils.GetPageFileName(pageModel.PageNumber));
        }

        //private void Page_OrientationChanged(object sender, OrientationChangedEventArgs e)
        //{
        //    if ((e.Orientation & PageOrientation.Landscape) == (PageOrientation.Landscape))
        //    {
        //        MainPivot.Height = screenInfo.ImageHeight;
        //    }
        //}

        // Sample code for building a localized ApplicationBar
        //private void BuildLocalizedApplicationBar()
        //{
        //    // Set the page's ApplicationBar to a new instance of ApplicationBar.
        //    ApplicationBar = new ApplicationBar();

        //    // Create a new button and set the text value to the localized string from AppResources.
        //    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
        //    appBarButton.Text = AppResources.AppBarButtonText;
        //    ApplicationBar.Buttons.Add(appBarButton);

        //    // Create a new menu item with the localized string from AppResources.
        //    ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
        //    ApplicationBar.MenuItems.Add(appBarMenuItem);
        //}
    }
}