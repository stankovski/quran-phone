using Cirrious.MvvmCross.Plugins.Sqlite;

namespace Quran.Core.Common
{
    public class ReciterItem
    {
        [Column("id"), PrimaryKey]
        public int Id { get; set; }
        [Column("name")]
        public string Name { get; set; }
        [Column("url")]
        public string ServerUrl { get; set; }
        [Column("local")]
        public string LocalPath { get; set; }
        [Column("dbname")]
        public string DatabaseName { get; set; }
        [Column("gapless")]
        public bool IsGapless { get; set; }
    }    
}
