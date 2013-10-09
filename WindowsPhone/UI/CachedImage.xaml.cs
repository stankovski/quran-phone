using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using QuranPhone.Common;
using QuranPhone.Data;
using QuranPhone.Utils;

namespace QuranPhone.UI
{
    public partial class CachedImage : UserControl, INotifyPropertyChanged, IDisposable
    {
        public static readonly DependencyProperty SelectedAyahProperty = DependencyProperty.Register("SelectedAyah",
            typeof (QuranAyah), typeof (CachedImage), new PropertyMetadata(ChangeSelectedAyah));

        public static readonly DependencyProperty NightModeProperty = DependencyProperty.Register("NightMode",
            typeof (bool), typeof (CachedImage), new PropertyMetadata(ChangeNightMode));

        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register("ImageSource",
            typeof (Uri), typeof (CachedImage), new PropertyMetadata(ChangeSource));

        public static readonly DependencyProperty PageNumberProperty = DependencyProperty.Register("PageNumber",
            typeof (int), typeof (CachedImage), new PropertyMetadata(ChangePageNumber));

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

        public Image Image
        {
            get { return image; }
        }

        public QuranAyah SelectedAyah
        {
            get { return (QuranAyah) GetValue(SelectedAyahProperty); }
            set { SetValue(SelectedAyahProperty, value); }
        }

        public bool NightMode
        {
            get { return (bool) GetValue(NightModeProperty); }
            set { SetValue(NightModeProperty, value); }
        }

        public Uri ImageSource
        {
            get { return (Uri) GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        public int PageNumber
        {
            get { return (int) GetValue(PageNumberProperty); }
            set { SetValue(PageNumberProperty, value); }
        }

        public void Dispose()
        {
            if (image != null)
            {
                image.Source = null;
            }
            imageSourceBitmap = null;
            imageSourceBitmapResized = null;
            ImageSource = null;
        }

        public event EventHandler<QuranAyahEventArgs> AyahTapped;

        private static void ChangeSelectedAyah(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            ((CachedImage) source).UpdateSelectedAyah(e.NewValue as QuranAyah);
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
                    if (basePath == null)
                    {
                        return;
                    }
                    string path = System.IO.Path.Combine(basePath, QuranFileUtils.GetAyaPositionFileName());
                    if (QuranFileUtils.FileExists(path))
                    {
                        int offsetToScrollTo = 0;
                        using (var dbh = new AyahInfoDatabaseHandler(QuranFileUtils.GetAyaPositionFileName()))
                        {
                            IList<AyahBounds> bounds = dbh.GetVerseBoundsCombined(ayahInfo.Sura, ayahInfo.Ayah);
                            if (bounds == null)
                            {
                                return;
                            }

                            // Reset any overlays
                            canvas.Children.Clear();
                            canvas.Opacity = 1.0;

                            foreach (AyahBounds bound in bounds)
                            {
                                DrawAyahBound(bound);
                                if (offsetToScrollTo == 0)
                                {
                                    offsetToScrollTo = bound.MinY;
                                }
                            }
                        }
                        LayoutRoot.ScrollToVerticalOffset(offsetToScrollTo);
                        canvasStoryboard.Begin();
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
            }
        }

        private static void ChangeNightMode(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            ((CachedImage) source).UpdateNightMode((bool) e.NewValue);
        }

        private void UpdateNightMode(bool isNightMode)
        {
            if (nightMode != isNightMode)
            {
                nightMode = isNightMode;
                UpdateSource(imageSourceUri, true);
            }
        }

        private static void ChangeSource(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            ((CachedImage) source).UpdateSource(e.NewValue as Uri, false);
        }

        private async void UpdateSource(Uri source, bool force)
        {
            if (imageSourceUri == source && !force)
            {
                return;
            }
            imageSourceUri = source;
            imageSourceBitmap = null;
            imageSourceBitmapResized = null;

            // Reset any overlays
            canvas.Children.Clear();

            // Scroll to top
            LayoutRoot.ScrollToVerticalOffset(0);

            progress.Visibility = Visibility.Visible;
            image.Source = null;

            if (source == null)
            {
                progress.Visibility = Visibility.Collapsed;
            }
            else
            {
                var uriBuilder = new UriBuilder(source);
                string localPath = System.IO.Path.Combine(QuranFileUtils.GetQuranDirectory(false),
                    System.IO.Path.GetFileName(uriBuilder.Path));
                bool downloadSuccessful = true;

                if (source.Scheme == "http")
                {
                    if (!QuranFileUtils.FileExists(localPath))
                    {
                        downloadSuccessful = await QuranFileUtils.DownloadFileFromWebAsync(source.ToString(), localPath);
                    }
                }
                else
                {
                    localPath = source.LocalPath;
                }

                try
                {
                    if (downloadSuccessful)
                    {
                        LoadImageFromLocalPath(localPath);
                    }
                }
                catch
                {
                    MessageBox.Show("Error loading quran page.");
                }
                finally
                {
                    progress.Visibility = Visibility.Collapsed;
                }

                if (PhoneUtils.IsPortaitOrientation && nightMode)
                {
                    image.Source = imageSourceBitmapResized;
                }
                else
                {
                    image.Source = imageSourceBitmap;
                }
            }
        }

        private void LoadImageFromLocalPath(string localPath)
        {
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (IsolatedStorageFileStream stream = isf.OpenFile(localPath, FileMode.Open))
                {
                    var bitmap = new WriteableBitmap(1, 1); 
                    bitmap.SetSource(stream);
                    if (nightMode)
                    {
                        imageSourceBitmapResized = ResizeBitmapIfNotExists(bitmap);
                        InvertColors(bitmap);
                        InvertColors(imageSourceBitmapResized);
                    }
                    imageSourceBitmap = bitmap;
                    progress.Visibility = Visibility.Collapsed;
                }
            }
        }

        private WriteableBitmap ResizeBitmapIfNotExists(WriteableBitmap bitmap)
        {
            if (imageSourceBitmapResized == null)
            {
                double hightToWidthRatio = (bitmap.PixelHeight/(double) bitmap.PixelWidth);

                return bitmap.Resize((int) Application.Current.Host.Content.ActualWidth,
                    (int) (Application.Current.Host.Content.ActualWidth*hightToWidthRatio),
                    WriteableBitmapExtensions.Interpolation.Bilinear);
            }
            return imageSourceBitmapResized;
        }

        private void InvertColors(WriteableBitmap bitmap)
        {
            int size = bitmap.Pixels.Length;
            for (int i = 0; i < size; i++)
            {
                int c = bitmap.Pixels[i];
                int a = 0x000000FF & (c >> 24);
                int r = 0x000000FF & (c >> 16);
                int g = 0x000000FF & (c >> 8);
                int b = 0x000000FF & (c);

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
            if (PhoneUtils.IsPortaitOrientation && nightMode)
            {
                image.Source = imageSourceBitmapResized;
            }
            else
            {
                image.Source = imageSourceBitmap;
            }
        }

        private static void ChangePageNumber(DependencyObject source, DependencyPropertyChangedEventArgs e) {}

        public static QuranAyah GetAyahFromGesture(Point p, int pageNumber, double width)
        {
            try
            {
                Point position = AdjustPoint(p, width);
                string basePath = QuranFileUtils.GetQuranDatabaseDirectory(false, true);
                if (basePath == null)
                {
                    return null;
                }
                string path = System.IO.Path.Combine(basePath, QuranFileUtils.GetAyaPositionFileName());
                if (QuranFileUtils.FileExists(path))
                {
                    using (var dbh = new AyahInfoDatabaseHandler(QuranFileUtils.GetAyaPositionFileName()))
                    {
                        return dbh.GetVerseAtPoint(pageNumber, position.X, position.Y);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            return null;
        }

        private void ImageTap(object sender, GestureEventArgs e)
        {
            if (AyahTapped != null)
            {
                QuranAyah ayah = GetAyahFromGesture(e.GetPosition(image), PageNumber, image.ActualWidth);
                if (ayah != null)
                {
                    AyahTapped(this, new QuranAyahEventArgs(ayah));
                }
            }
        }

        private static Point AdjustPoint(Point p, double width)
        {
            int imageWidth = QuranScreenInfo.Instance.ImageWidth;
            double actualWidth = width;
            double scale = imageWidth/actualWidth;
            return new Point(p.X*scale, p.Y*scale);
        }

        private void DrawAyahBound(AyahBounds bound)
        {
            var myPointCollection = new PointCollection();
            myPointCollection.Add(new Point(bound.MinX, bound.MinY));
            myPointCollection.Add(new Point(bound.MaxX, bound.MinY));
            myPointCollection.Add(new Point(bound.MaxX, bound.MaxY));
            myPointCollection.Add(new Point(bound.MinX, bound.MaxY));

            var myPolygon = new Polygon();
            myPolygon.Points = myPointCollection;
            myPolygon.Fill = new SolidColorBrush(Color.FromArgb(50, 48, 182, 231));
            canvas.Children.Add(myPolygon);
        }

        #region INotifyPropertyChanged Members

        /// <summary>
        ///     Raised when a property on this object has a new value.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Raises this object's PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The property that has a new value.</param>
        protected virtual void OnPropertyChanged<T>(Expression<Func<T>> expression)
        {
            string propertyName = GetPropertyName(expression);
            OnPropertyChanged(propertyName);
        }

        /// <summary>
        ///     Raises this object's PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The property that has a new value.</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
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
        ///     Get the string name for the property
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        protected string GetPropertyName<T>(Expression<Func<T>> expression)
        {
            var memberExpression = (MemberExpression) expression.Body;
            return memberExpression.Member.Name;
        }

        #endregion // INotifyPropertyChanged Members
    }
}