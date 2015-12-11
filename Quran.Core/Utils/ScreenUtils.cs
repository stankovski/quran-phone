using Quran.Core.Common;

namespace Quran.Core.Utils
{
    public class ScreenUtils
    {

        private static ScreenUtils instance = null;

        private double _width;
        private double _height;
        private double _max_width;

        private ScreenUtils(double width, double height, double scale)
        {
            _width = width * scale;
            _height = height * scale;
            _max_width = (_width > _height) ? _width : _height;
        }

        public static ScreenUtils Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ScreenUtils(QuranApp.NativeProvider.ActualWidth, 
                        QuranApp.NativeProvider.ActualHeight, QuranApp.NativeProvider.ScaleFactor);
                }
                return instance;
            }
        }

        public double Width { get { return this._width; }}
        public double Height { get { return this._height; } }
        public int ImageWidth { 
            get 
            {
                if (this._max_width <= 320) return 320;
                else if (this._max_width <= 480) return 480;
                else if (this._max_width <= 800) return 800;
                else return 1024; 
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

        public string GetWidthParam()
        {
            return "_" + GetWidthParamNoUnderScore();
        }

        public string GetWidthParamNoUnderScore()
        {
            if (this._max_width <= 320) return "320";
            else if (this._max_width <= 480) return "480";
            else if (this._max_width <= 800) return "800";
            else return "1024";
        }
    }
}
