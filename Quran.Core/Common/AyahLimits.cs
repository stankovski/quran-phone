using SQLite.Net.Attributes;

namespace Quran.Core.Common
{
    [Table("glyphs")]
    public class AyahLimits
    {
        [Column("MIN(min_x)")]
        public int MinX { get; set; }
        [Column("MIN(min_y)")]
        public int MinY { get; set; }
        [Column("MAX(max_x)")]
        public int MaxX { get; set; }
        [Column("MAX(max_y)")]
        public int MaxY { get; set; }
        [Column("page_number")]
        public int PageNumber { get; set; }
    }
}
