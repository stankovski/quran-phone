using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuranPhone.Common
{
    public class AyahBounds{
        public AyahBounds(int line,int position,int minX,int minY,int maxX,int maxY) 
        {        
            Line = line; 
            Position = position;
            MinX = minX;
            MinY = minY;
            MaxX = maxX;
            MaxY = maxY;
        }

        public void engulf(AyahBounds other)
        {
		    if (MinX > other.MinX)
			    MinX = other.MinX;
		    if (MinY > other.MinY)
			    MinY = other.MinY;
		    if (MaxX < other.MaxX)
			    MaxX = other.MaxX;
		    if (MaxY < other.MaxY)
			    MaxY = other.MaxY;
	    }

        public int MinX { get; set; }
        public int MinY { get; set; }
        public int MaxX { get; set; }
        public int MaxY { get; set; }
        public int Line { get; set; }
        public int Position { get; set; }
        
    }
}
