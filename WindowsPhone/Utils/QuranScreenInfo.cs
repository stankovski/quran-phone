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

        private double width;
        private double height;
        private double max_width;
        private PageOrientation orientation;

        private QuranScreenInfo(double width, double height)
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
                instance = new QuranScreenInfo(Application.Current.Host.Content.ActualWidth,
                Application.Current.Host.Content.ActualHeight);
            }
            return instance;
        }

        public double Width { get { return this.width; }}
        public double Height { get { return this.height; } }

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
