using Cirrious.MvvmCross.Plugins.Sqlite;
using System;

namespace Quran.Core.Common
{
    [Table("tags")]
    public class Tags
    {
        [Column("_ID"), PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Column("name")]
        public string Name { get; set; }
        [Column("added_date")]
        public DateTime AddedDate { get; set; }
        [Ignore]
        public bool Checked { get; set; }
        public void Toggle()
        {
            Checked = !Checked;
        }
    }
}
