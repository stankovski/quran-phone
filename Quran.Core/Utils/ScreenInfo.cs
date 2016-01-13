using System.Runtime.Serialization;
using Quran.Core.Common;

namespace Quran.Core.Utils
{
    [DataContract]
    public class ScreenInfo
    {
        private static ScreenInfo instance = null;

        private ScreenInfo()
        {}

        private ScreenInfo(double width, double height, double scale)
        {
            Width = width * scale;
            Height = height * scale;
        }

        public static ScreenInfo Instance
        {
            get
            {
                if (instance == null && QuranApp.NativeProvider != null)
                {
                    instance = new ScreenInfo(QuranApp.NativeProvider.ActualWidth, 
                        QuranApp.NativeProvider.ActualHeight, QuranApp.NativeProvider.ScaleFactor);
                }
                return instance;
            }
        }

        [DataMember]
        public double Width { get; set; }

        [DataMember]
        public double Height { get; set; }

        public double MaxWidth
        {
            get
            {
                return (Width > Height) ? Width : Height;
            }
        }

        public int ImageWidth { 
            get 
            {
                if (this.MaxWidth <= 320) return 320;
                else if (this.MaxWidth <= 480) return 480;
                else if (this.MaxWidth <= 800) return 800;
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
            if (this.MaxWidth <= 320) return "320";
            else if (this.MaxWidth <= 480) return "480";
            else if (this.MaxWidth <= 800) return "800";
            else return "1024";
        }
    }
}
