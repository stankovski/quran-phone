using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Controls;
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

        private WriteableBitmap imageSourceBitmap;
        private WriteableBitmap imageSourceBitmapResized;
        private Uri imageSourceUri;
        private bool nightMode;

        public CachedImage()
        {
            imageSourceBitmap = new WriteableBitmap(1, 1);
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
                    string path = Path.Combine(basePath, QuranFileUtils.GetAyaPositionFileName());
                    if (QuranFileUtils.FileExists(path))
                    {
                        using (var dbh = new AyahInfoDatabaseHandler(QuranFileUtils.GetAyaPositionFileName()))
                        {
                            var bounds = dbh.GetVerseBoundsCombined(ayahInfo.Sura, ayahInfo.Ayah);
                            // Reset any overlays
                            canvas.Children.Clear();
                            canvas.Opacity = 1.0;

                            foreach (var bound in bounds)
                            {
                                drawAyahBound(bound);
                            }
                        }
                        canvasStoryboard.Begin();
                    }
                }
                catch
                {
                    //Ignore
                }
            }
        }

        public bool NightMode
        {
            get { return (bool)GetValue(NightModeProperty); }
            set { SetValue(NightModeProperty, value); }
        }

        public static readonly DependencyProperty NightModeProperty = DependencyProperty.Register("NightMode",
            typeof(bool), typeof(CachedImage), new PropertyMetadata(
            new PropertyChangedCallback(changeNightMode)));

        private static void changeNightMode(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            (source as CachedImage).UpdateNightMode((bool)e.NewValue);
        }

        private void UpdateNightMode(bool isNightMode)
        {
            if (this.nightMode != isNightMode)
            {
                this.nightMode = isNightMode;
                UpdateSource(imageSourceUri, true);
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
            (source as CachedImage).UpdateSource(e.NewValue as Uri, false); 
        }

        private async void UpdateSource(Uri source, bool force)
        {
            if (imageSourceUri == source && !force)
            {
                return;
            }
            else
            {
                imageSourceUri = source;
                imageSourceBitmap = null;
                imageSourceBitmapResized = null;
            }

            // Reset any overlays
            canvas.Children.Clear();

            // Scroll to top
            LayoutRoot.ScrollToVerticalOffset(0);

            progress.Visibility = System.Windows.Visibility.Visible;
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
                    imageSourceBitmap = new WriteableBitmap(1,1);
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
                        throw new Exception();
                }
                catch
                {
                    MessageBox.Show("Error loading quran page.");
                }
                finally
                {
                    progress.Visibility = System.Windows.Visibility.Collapsed;
                    progress.IsIndeterminate = false;
                }

                if (IsPortrate && nightMode)
                    image.Source = imageSourceBitmapResized;
                else 
                    image.Source = imageSourceBitmap;
            }
        }

        private void loadImageFromLocalPath(string localPath)
        {
            using (var isf = IsolatedStorageFile.GetUserStoreForApplication())
            using (var stream = isf.OpenFile(localPath, FileMode.Open))
            {
                var bitmap = new WriteableBitmap(1, 1); // avoid creating intermediate BitmapImage
                bitmap.SetSource(stream);
                if (nightMode)
                {
                    imageSourceBitmapResized = resizeBitmapIfNotExists(bitmap);
                    invertColors(bitmap);
                    invertColors(imageSourceBitmapResized);
                }
                imageSourceBitmap = bitmap;
                progress.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private WriteableBitmap resizeBitmapIfNotExists(WriteableBitmap bitmap)
        {
            if (imageSourceBitmapResized == null)
            {
                var hightToWidthRatio = ((double)bitmap.PixelHeight / (double)bitmap.PixelWidth);

                return bitmap.Resize((int) Application.Current.Host.Content.ActualWidth,
                                     (int) (Application.Current.Host.Content.ActualWidth*hightToWidthRatio),
                                     WriteableBitmapExtensions.Interpolation.Bilinear);
            }
            else
            {
                return imageSourceBitmapResized;
            }
        }

        private void invertColors(WriteableBitmap bitmap)
        {
            int size = bitmap.Pixels.Length;
                for (int i = 0; i < size; i++)
                {
                var c = bitmap.Pixels[i];
                    var a = 0x000000FF & (c >> 24);
                    var r = 0x000000FF & (c >> 16);
                    var g = 0x000000FF & (c >> 8);
                    var b = 0x000000FF & (c);

                    // Invert
                    r = 0x000000FF & (0xFF - r);
                    g = 0x000000FF & (0xFF - g);
                    b = 0x000000FF & (0xFF - b);

                    // Set result color
                bitmap.Pixels[i] = (a << 24) | (r << 16) | (g << 8) | b;
            }
        }

        private void SizeChange(object sender, SizeChangedEventArgs e)
        {
            if (IsPortrate && nightMode)
                image.Source = imageSourceBitmapResized;
            else
                image.Source = imageSourceBitmap;
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

        private bool IsPortrate
        {
            get
            {
                return ((PhoneApplicationFrame)Application.Current.RootVisual).Orientation == PageOrientation.Portrait ||
                       ((PhoneApplicationFrame)Application.Current.RootVisual).Orientation ==
                       PageOrientation.PortraitUp ||
                       ((PhoneApplicationFrame)Application.Current.RootVisual).Orientation ==
                       PageOrientation.PortraitDown;
            }
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
            if (image != null)
                image.Source = null;
            imageSourceBitmap = null;
            imageSourceBitmapResized = null;
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
                string path = Path.Combine(basePath, QuranFileUtils.GetAyaPositionFileName());
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
