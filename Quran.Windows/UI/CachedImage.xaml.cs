using System;
using System.ComponentModel;
using System.Linq.Expressions;
using Quran.Core;
using Quran.Core.Common;
using Quran.Core.Utils;
using Quran.Windows.Utils;
using Quran.Core.Data;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml;
using Windows.Foundation;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using Windows.UI;
using Windows.UI.Xaml.Input;
using System.Threading.Tasks;
using Windows.Storage;
using System.Runtime.InteropServices.WindowsRuntime;

namespace Quran.Windows.UI
{
    public partial class CachedImage : UserControl, INotifyPropertyChanged, IDisposable
    {
        public event EventHandler<QuranAyahEventArgs> AyahTapped;

        private WriteableBitmap imageSourceBitmap;
        private Uri imageSourceUri;

        public CachedImage()
        {
            imageSourceBitmap = new WriteableBitmap(1, 1);
            InitializeComponent();
            canvas.Width = ScreenUtils.Instance.ImageWidth;
            canvas.Height = ScreenUtils.Instance.ImageHeight;
        }

        public Image Image
        {
            get { return image; }
        }

        public Visibility FooterVisibility
        {
            get { return (Visibility)GetValue(FooterVisibilityProperty); }
            set { SetValue(FooterVisibilityProperty, value); }
        }

        public static readonly DependencyProperty FooterVisibilityProperty = DependencyProperty.Register("FooterVisibility",
            typeof(Visibility), typeof(CachedImage), new PropertyMetadata(
            new PropertyChangedCallback(changeFooterVisibility)));

        private static void changeFooterVisibility(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            (source as CachedImage).ExpanderGrid.Visibility = (Visibility)e.NewValue;
        }

        public QuranAyah SelectedAyah
        {
            get { return (QuranAyah)GetValue(SelectedAyahProperty); }
            set { SetValue(SelectedAyahProperty, value); }
        }

        public static readonly DependencyProperty SelectedAyahProperty = DependencyProperty.Register("SelectedAyah",
            typeof(QuranAyah), typeof(CachedImage), new PropertyMetadata(null, ChangeSelectedAyah));

        private static async void ChangeSelectedAyah(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            await (source as CachedImage).UpdateSelectedAyah(e.NewValue as QuranAyah);
        }

        private async Task UpdateSelectedAyah(QuranAyah ayahInfo)
        {
            if (ayahInfo == null)
            {
                canvas.Children.Clear();
            }
            else
            {
                try
                {
                    string basePath = FileUtils.GetQuranDatabaseDirectory();
                    if (basePath == null) return;
                    string path = System.IO.Path.Combine(basePath, FileUtils.GetAyaPositionFileName());
                    if (await FileUtils.FileExists(path))
                    {
                        int offsetToScrollTo = 0;
                        using (var dbh = new AyahInfoDatabaseHandler(FileUtils.GetAyaPositionFileName()))
                        {
                            var bounds = dbh.GetVerseBoundsCombined(ayahInfo.Surah, ayahInfo.Ayah);
                            if (bounds == null)
                                return;

                            // Reset any overlays
                            canvas.Children.Clear();
                            canvas.Opacity = 1.0;

                            foreach (var bound in bounds)
                            {
                                drawAyahBound(bound);
                                if (offsetToScrollTo == 0)
                                    offsetToScrollTo = bound.MinY;
                            }
                        }
                        var adjustedScrollPoint = adjustPointRevert(new Point(1, offsetToScrollTo), LayoutRoot.ActualWidth);
                        LayoutRoot.ScrollToVerticalOffset(adjustedScrollPoint.Y); //Adjusting for ViewBox offset
                        if (QuranApp.DetailsViewModel.AudioPlayerState == AudioState.Playing)
                        {
                            canvasStoryboard.Seek(new TimeSpan(1));
                            canvasStoryboard.Stop();
                        }
                        else
                        {
                            canvasStoryboard.Begin();
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
            typeof(Uri), typeof(CachedImage), new PropertyMetadata(null, changeSource)); 
        
        private static async void changeSource(DependencyObject source, DependencyPropertyChangedEventArgs e) 
        { 
            await (source as CachedImage).UpdateSource(e.NewValue as Uri, false); 
        }

        private async Task UpdateSource(Uri source, bool force)
        {
            if (imageSourceUri == source && !force)
            {
                return;
            }
            else
            {
                imageSourceUri = source;
                imageSourceBitmap = null;
            }

            // Reset any overlays
            canvas.Children.Clear();

            // Scroll to top
            LayoutRoot.ScrollToVerticalOffset(0);

            progress.Visibility = Visibility.Visible;
            image.Source = null;
            if (source == null)
            {
                progress.Visibility = Visibility.Collapsed;
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
                var localPath = System.IO.Path.Combine(FileUtils.GetQuranDirectory(), System.IO.Path.GetFileName(uriBuilder.Path));
                bool downloadSuccessful = true;

                if (source.Scheme == "http")
                {
                    try
                    {
                        if (!await FileUtils.FileExists(localPath))
                        {
                            downloadSuccessful =
                                await FileUtils.DownloadFileFromWebAsync(source.ToString(), localPath);
                        }
                    }
                    catch (Exception ex)
                    {
                        await QuranApp.NativeProvider.ShowErrorMessageBox("Error loading quran page:" + ex.ToString());
                        downloadSuccessful = false;
                    }

                }
                else
                {
                    localPath = source.LocalPath;
                }

                try
                {
                    if (downloadSuccessful)
                        await loadImageFromLocalPath(localPath);
                    else
                        throw new Exception();
                }
                catch
                {
                    await QuranApp.NativeProvider.ShowErrorMessageBox("Error loading quran page.");
                    await FileUtils.SafeFileDelete(localPath);
                }
                finally
                {
                    progress.Visibility = Visibility.Collapsed;
                    progress.IsIndeterminate = false;
                }

                image.Source = imageSourceBitmap;
            }
        }

        private async Task loadImageFromLocalPath(string localPath)
        {
            var imageFile = await StorageFile.GetFileFromPathAsync(localPath);
            using (var imageFileStream = await imageFile.OpenReadAsync())
            {
                var bitmap = new WriteableBitmap(1, 1); // avoid creating intermediate BitmapImage
                await bitmap.SetSourceAsync(imageFileStream);
                if (SettingsUtils.Get<bool>(Constants.PREF_NIGHT_MODE))
                {
                    try
                    {
                        await InvertColors(bitmap);
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                }
                imageSourceBitmap = bitmap;
                progress.Visibility = Visibility.Collapsed;
            }
        }

        private async Task InvertColors(WriteableBitmap bitmap)
        {
            byte[] imageArray = new byte[bitmap.PixelHeight * bitmap.PixelWidth * 4];
            using (var stream = bitmap.PixelBuffer.AsStream())
            {
                await stream.ReadAsync(imageArray, 0, imageArray.Length);
                for (int i = 0; i < imageArray.Length; i += 4)
                {
                    var a = imageArray[i + 3]; // alpha
                    var r = imageArray[i + 2]; // red
                    var g = imageArray[i + 1]; // green
                    var b = imageArray[i]; // blue

                    // Invert
                    //r = 0x000000FF & (0xFF - r);
                    //g = 0x000000FF & (0xFF - g);
                    //b = 0x000000FF & (0xFF - b);
                    if (a > 0)
                    {
                        r = (byte)(0xFF - r);
                        g = (byte)(0xFF - g);
                        b = (byte)(0xFF - b);
                    }

                    // Set result color
                    imageArray[i] = b;
                    imageArray[i + 1] = g;
                    imageArray[i + 2] = r;
                    imageArray[i + 3] = 255;
                }
                stream.Position = 0;
                await stream.WriteAsync(imageArray, 0, imageArray.Length);
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
        protected virtual void RaisePropertyChanged<T>(Expression<Func<T>> expression)
        {
            string propertyName = GetPropertyName(expression);
            RaisePropertyChanged(propertyName);
        }

        /// <summary>
        /// Raises this object's PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The property that has a new value.</param>
        protected virtual void RaisePropertyChanged(string propertyName)
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

        public static async Task<QuranAyah> GetAyahFromGesture(Point p, int pageNumber, double width)
        {
            try
            {
                var position = adjustPoint(p, width);
                string basePath = FileUtils.GetQuranDatabaseDirectory();
                if (basePath == null) 
                    return null;
                string path = System.IO.Path.Combine(basePath, FileUtils.GetAyaPositionFileName());
                if (await FileUtils.FileExists(path))
                {
                    using (var dbh = new AyahInfoDatabaseHandler(FileUtils.GetAyaPositionFileName()))
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

        private async void ImageTap(object sender, TappedRoutedEventArgs e)
        {
            if (AyahTapped != null)
            {
                var ayah = await GetAyahFromGesture(e.GetPosition(image), PageNumber, image.ActualWidth);
                if (ayah != null)
                    AyahTapped(this, new QuranAyahEventArgs(ayah));
            }
        }

        private static Point adjustPoint(Point p, double width)
        {
            var imageWidth = ScreenUtils.Instance.ImageWidth;
            var actualWidth = width;
            var scale = imageWidth/actualWidth;
            return new Point(p.X*scale, p.Y*scale);
        }

        private static Point adjustPointRevert(Point p, double width)
        {
            var imageWidth = ScreenUtils.Instance.ImageWidth;
            var actualWidth = width;
            var scale = imageWidth / actualWidth;
            return new Point(p.X / scale, p.Y / scale);
        }

        private void drawAyahBound(AyahBounds bound)
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

        public void Dispose()
        {
            if (image != null)
                image.Source = null;
            imageSourceBitmap = null;
            ImageSource = null;
            #if DEBUG
            string msg = string.Format("{0} ({1}) Disposed", this.GetType().Name, this.GetHashCode());
            System.Diagnostics.Debug.WriteLine(msg);
            #endif
        }

        #if DEBUG
        /// <summary>
        /// Useful for ensuring that ViewModel objects are properly garbage collected.
        /// </summary>
        ~CachedImage()
        {
            string msg = string.Format("{0} ({1}) Finalized", this.GetType().Name, this.GetHashCode());
            System.Diagnostics.Debug.WriteLine(msg);
        }
        #endif
    }
}
