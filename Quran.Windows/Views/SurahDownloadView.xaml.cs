using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Quran.Core.Utils;
using Quran.Core.ViewModels;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Quran.Windows.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SurahDownloadView : Page
    {
        public SurahDownloadViewModel ViewModel { get; set; }

        public SurahDownloadView()
        {
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.None;
            ViewModel = new SurahDownloadViewModel();
            this.InitializeComponent();
        }

        // When page is navigated to set data context to selected item in list
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            int? reciter = e.Parameter as int?;
            if (reciter == null)
            {
                throw new ArgumentNullException("reciter");
            }

            ViewModel.Reciter = AudioUtils.GetReciterById(reciter.Value);
            await ViewModel.Initialize();
        }

        private void NavigationRequested(object sender, TappedRoutedEventArgs e)
        {

        }
    }
}
