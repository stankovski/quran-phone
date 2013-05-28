using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Windows.Media.Imaging;
using QuranPhone.Utils;
using ImageTools;
using System.IO.IsolatedStorage;
using System.IO;
using System.Diagnostics;

namespace QuranPhone.UI
{
    public partial class CachedImage : UserControl, INotifyPropertyChanged, IDisposable
    {
        private BitmapImage imageSourceBitmap;
        private Uri imageSourceUri; 

        public CachedImage()
        {
            imageSourceBitmap = new BitmapImage();
            imageSourceBitmap.CreateOptions = BitmapCreateOptions.DelayCreation;
            InitializeComponent();
        }

        public Uri ImageSource {
            get { return (Uri)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); } 
        }

        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register("ImageSource",
            typeof(Uri), typeof(CachedImage), new PropertyMetadata(
            new PropertyChangedCallback(changeSource))); 
        
        private static void changeSource(DependencyObject source, DependencyPropertyChangedEventArgs e) 
        { 
            (source as CachedImage).UpdateSource(e.NewValue as Uri); 
        }

        private async void UpdateSource(Uri source)
        {
            if (imageSourceUri == source)
            {
                return;
            }
            else
            {
                imageSourceUri = source;
            }

            progress.Visibility = System.Windows.Visibility.Visible;
            imageSourceBitmap.UriSource = null;
            image.Source = null;
            if (source == null)
            {
                progress.Visibility = System.Windows.Visibility.Collapsed;
                progress.IsIndeterminate = false;
            }
            else
            {
                var uriBuilder = new UriBuilder(source);
                var localPath = Path.Combine(QuranFileUtils.GetQuranDirectory(false), Path.GetFileName(uriBuilder.Path));
                bool downloadSuccessful = true;

                if (source.Scheme == "http")
                {
                    if (!QuranFileUtils.FileExists(localPath))
                        downloadSuccessful = await QuranFileUtils.DownloadFileFromWebAsync(source.ToString(), localPath);
                }
                else
                {
                    localPath = source.LocalPath;
                }

                try
                {
                    if (downloadSuccessful)
                        loadImageFromLocalPath(localPath);
                    else
                        loadImageFromRemotePath(source);
                }
                catch
                {
                    loadImageFromRemotePath(source);
                }
                finally
                {
                    progress.Visibility = System.Windows.Visibility.Collapsed;
                    progress.IsIndeterminate = false;
                }

                image.Source = imageSourceBitmap;

                // Scroll to the top of ScrollView after updating image
                LayoutRoot.ScrollToVerticalOffset(0);
            }
        }

        private void loadImageFromRemotePath(Uri source)
        {
            var backupSource = QuranFileUtils.GetImageFromWeb(Path.GetFileName(source.LocalPath), false);
            imageSourceBitmap.UriSource = backupSource;
            ImageSource = backupSource;
        }

        private void loadImageFromLocalPath(string localPath)
        {
            using (var isf = IsolatedStorageFile.GetUserStoreForApplication())
            using (var stream = isf.OpenFile(localPath, FileMode.Open))
            {
                imageSourceBitmap.SetSource(stream);
                progress.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        public int PageNumber
        {
            get { return (int)GetValue(PageNumberProperty); }
            set { SetValue(PageNumberProperty, value); }
        }

        public static readonly DependencyProperty PageNumberProperty = DependencyProperty.Register("PageNumber",
            typeof(int), typeof(CachedImage), new PropertyMetadata(
            new PropertyChangedCallback(changePageNumber)));

        private static void changePageNumber(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            (source as CachedImage).UpdatePageNumber((int)e.NewValue);
        }

        private void UpdatePageNumber(int source)
        {
            //pageNumber.Text = "Page " + source;
        }

        
        #region INotifyPropertyChanged Members

        /// <summary>
        /// Raised when a property on this object has a new value.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises this object's PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The property that has a new value.</param>
        protected virtual void OnPropertyChanged<T>(Expression<Func<T>> expression)
        {
            string propertyName = GetPropertyName(expression);
            OnPropertyChanged(propertyName);
        }

        /// <summary>
        /// Raises this object's PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The property that has a new value.</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        protected virtual bool IsProperty<T>(Expression<Func<T>> expression, string name)
        {
            string propertyName = GetPropertyName(expression);
            return propertyName == name;
        }

        /// <summary>
        /// Get the string name for the property
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        protected string GetPropertyName<T>(Expression<Func<T>> expression)
        {
            MemberExpression memberExpression = (MemberExpression)expression.Body;
            return memberExpression.Member.Name;
        }

        #endregion // INotifyPropertyChanged Members

        private void UserControl_MouseEnter_1(object sender, System.Windows.Input.MouseEventArgs e)
        {
            memoryUsage.Text = PhoneUtils.CurrentMemoryUsage();
        }

        public void Dispose()
        {
            imageSourceBitmap.UriSource = null;
            if (image != null)
                image.Source = null;
            imageSourceBitmap = null;
            ImageSource = null;
        }
    }
}
