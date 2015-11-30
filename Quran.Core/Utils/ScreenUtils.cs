using Quran.Core.Common;

namespace Quran.Core.Utils
{
    public class ScreenUtils
    {

        private static ScreenUtils instance = null;

        private int width;
        private int height;
        private int max_width;

        private ScreenUtils(int width, int height)
        {
            this.width = width;
            this.height = height;
            this.max_width = (width > height) ? width : height;
        }

        public static ScreenUtils Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ScreenUtils((int)QuranApp.NativeProvider.ActualWidth, (int)QuranApp.NativeProvider.ActualHeight);
                }
                return instance;
            }
        }

        public int Width { get { return this.width; }}
        public int Height { get { return this.height; } }
        public int ImageWidth { 
            get 
            {
                if (this.max_width <= 320) return 320;
                else if (this.max_width <= 480) return 480;
                else if (this.max_width <= 800) return 800;
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
            if (this.max_width <= 320) return "320";
            else if (this.max_width <= 480) return "480";
            else if (this.max_width <= 800) return "800";
            else return "1024";
        }
    }
}
