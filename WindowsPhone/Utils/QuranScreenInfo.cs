using System.Windows;

namespace QuranPhone.Utils
{
    public class QuranScreenInfo
    {
        private double _height;
        private double _maxWidth;
        private double _width;

        private QuranScreenInfo(double width, double height)
        {
            _width = width;
            _height = height;
            _maxWidth = (width > height) ? width : height;
        }

        public static QuranScreenInfo Instance
        {
            get
            {
                return new QuranScreenInfo(Application.Current.Host.Content.ActualWidth,
                    Application.Current.Host.Content.ActualHeight);
            }
        }

        public double Width
        {
            get { return _width; }
        }

        public double Height
        {
            get { return _height; }
        }

        public int ImageWidth
        {
            get
            {
                if (_maxWidth <= 320)
                {
                    return 320;
                }
                if (_maxWidth <= 480)
                {
                    return 480;
                }
                if (_maxWidth <= 800)
                {
                    return 800;
                }
                return 1024;
            }
        }

        public int ImageHeight
        {
            get
            {
                switch (ImageWidth)
                {
                    case 320:
                        return 517;
                    case 480:
                        return 776;
                    case 800:
                        return 1294;
                    default:
                        return 1656;
                }
            }
        }

        public string GetWidthParamWithUnderScore()
        {
            return "_" + GetWidthParamNoUnderScore();
        }

        public string GetWidthParamNoUnderScore()
        {
            if (_maxWidth <= 320)
            {
                return "320";
            }
            if (_maxWidth <= 480)
            {
                return "480";
            }
            if (_maxWidth <= 800)
            {
                return "800";
            }
            return "1024";
        }
    }
}