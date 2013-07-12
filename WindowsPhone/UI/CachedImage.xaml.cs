using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Windows.Media.Imaging;
using QuranPhone.Common;
using QuranPhone.Data;
using QuranPhone.Utils;
using System.IO.IsolatedStorage;
using System.IO;
using Path = System.IO.Path;

namespace QuranPhone.UI
{
    public partial class CachedImage : UserControl, INotifyPropertyChanged, IDisposable
    {
        public event EventHandler<QuranAyahEventArgs> AyahTapped;

        private BitmapImage imageSourceBitmap;
        private Uri imageSourceUri; 

        public CachedImage()
        {
            imageSourceBitmap = new BitmapImage();
            imageSourceBitmap.CreateOptions = BitmapCreateOptions.DelayCreation;
            InitializeComponent();
            canvas.Width = QuranScreenInfo.Instance.ImageWidth;
            canvas.Height = QuranScreenInfo.Instance.ImageHeight;
        }

        public QuranAyah SelectedAyah
        {
            get { return (QuranAyah)GetValue(SelectedAyahProperty); }
            set { SetValue(SelectedAyahProperty, value); }
        }

        public static readonly DependencyProperty SelectedAyahProperty = DependencyProperty.Register("SelectedAyah",
            typeof(QuranAyah), typeof(CachedImage), new PropertyMetadata(
            new PropertyChangedCallback(changeSelectedAyah)));

        private static void changeSelectedAyah(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            (source as CachedImage).UpdateSelectedAyah(e.NewValue as QuranAyah);
        }

        private void UpdateSelectedAyah(QuranAyah ayahInfo)
        {
            if (ayahInfo == null)
            {
                canvas.Children.Clear();
            }
            else
            {
                try
                {
                    string basePath = QuranFileUtils.GetQuranDatabaseDirectory(false, true);
                    if (basePath == null) return;
                    string path = basePath + QuranFileUtils.PATH_SEPARATOR + QuranFileUtils.GetAyaPositionFileName();
                    if (QuranFileUtils.FileExists(path))
                    {
                        using (var dbh = new AyahInfoDatabaseHandler(QuranFileUtils.GetAyaPositionFileName()))
                        {
                            var bounds = dbh.GetVerseBoundsCombined(ayahInfo.Sura, ayahInfo.Ayah);
                            // Reset any overlays
                            canvas.Children.Clear();

                            foreach (var bound in bounds)
                            {
                                drawAyahBound(bound);
                            }
                        }
                    }
                }
                catch
                {
                    //Ignore
                }
            }
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

            // Reset any overlays
            canvas.Children.Clear();

            // Scroll to top
            LayoutRoot.ScrollToVerticalOffset(0);

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
                // Design time preview
                if (PhoneUtils.IsDesignMode)
                {
                    imageSourceBitmap.UriSource = source;
                    ImageSource = source;
                    return;
                }

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

        public void Dispose()
        {
            imageSourceBitmap.UriSource = null;
            if (image != null)
                image.Source = null;
            imageSourceBitmap = null;
            ImageSource = null;
        }

        public static QuranAyah GetAyahFromGesture(Point p, int pageNumber, double width)
        {
            try
            {
                var position = adjustPoint(p, width);
                string basePath = QuranFileUtils.GetQuranDatabaseDirectory(false, true);
                if (basePath == null) 
                    return null;
                string path = basePath + QuranFileUtils.PATH_SEPARATOR + QuranFileUtils.GetAyaPositionFileName();
                if (QuranFileUtils.FileExists(path))
                {
                    using (var dbh = new AyahInfoDatabaseHandler(QuranFileUtils.GetAyaPositionFileName()))
                    {
                        return dbh.GetVerseAtPoint(pageNumber, position.X, position.Y);
                    }
                }
            }
            catch
            {
                // Ignore
            }
            return null;
        }

        private void ImageTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (AyahTapped != null)
            {
                var ayah = GetAyahFromGesture(e.GetPosition(image), PageNumber, image.ActualWidth);
                if (ayah != null)
                    AyahTapped(this, new QuranAyahEventArgs(ayah));
            }
        }

        private static Point adjustPoint(Point p, double width)
        {
            var imageWidth = QuranScreenInfo.Instance.ImageWidth;
            var actualWidth = width;
            var scale = imageWidth/actualWidth;
            return new Point(p.X*scale, p.Y*scale);
        }

        private void drawAyahBound(Common.AyahBounds bound)
        {
            PointCollection myPointCollection = new PointCollection();
            myPointCollection.Add(new Point(bound.MinX, bound.MinY));
            myPointCollection.Add(new Point(bound.MaxX, bound.MinY));
            myPointCollection.Add(new Point(bound.MaxX, bound.MaxY));
            myPointCollection.Add(new Point(bound.MinX, bound.MaxY));

            Polygon myPolygon = new Polygon();
            myPolygon.Points = myPointCollection;
            myPolygon.Fill = new SolidColorBrush(Color.FromArgb(50, 48, 182, 231));
            canvas.Children.Add(myPolygon);
        }
    }
}
