using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace QuranPhone.Utils
{
    public class QuranScreenInfo
    {

        private static QuranScreenInfo instance = null;

        private int width;
        private int height;
        private int max_width;
        private PageOrientation orientation;

        private QuranScreenInfo(int width, int height)
        {
            this.orientation = PageOrientation.Portrait;
            this.width = width;
            this.height = height;
            this.max_width = (width > height) ? width : height;
        }

        public static QuranScreenInfo GetInstance()
        {
            if (instance == null)
            {
                instance = new QuranScreenInfo((int)Application.Current.Host.Content.ActualWidth,
                (int)Application.Current.Host.Content.ActualHeight);
            }
            return instance;
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

        public bool IsLandscapeOrientation()
        {
            return (orientation == PageOrientation.Landscape ||
                orientation == PageOrientation.LandscapeLeft ||
                orientation == PageOrientation.LandscapeRight);
        }

        public void SetOrientation(PageOrientation orientation)
        {
            this.orientation = orientation;
        }
    }
}
