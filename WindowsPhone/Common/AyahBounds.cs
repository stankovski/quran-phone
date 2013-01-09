using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuranPhone.Common
{
    [Table("glyphs")]
    public class AyahBounds
    {
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

        [Column("min_x")]
        public int MinX { get; set; }
        [Column("min_y")]
        public int MinY { get; set; }
        [Column("max_x")]
        public int MaxX { get; set; }
        [Column("max_y")]
        public int MaxY { get; set; }
        [Column("line_number")]
        public int Line { get; set; }
        [Column("position")]
        public int Position { get; set; }
        [Column("page_number")]
        public int PageNumber { get; set; }
        [Column("sura_number")]
        public int SurahNumber { get; set; }
        [Column("ayah_number")]
        public int AyahNumber { get; set; }
    }
}
