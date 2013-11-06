using Cirrious.MvvmCross.Plugins.Sqlite;

namespace Quran.Core.Common
{
    [Table("properties")]
    public class DatabaseProperties
    {
        [Column("property")]
        public string Property { get; set; }
        [Column("value")]
        public string Value { get; set; }
    }
}
