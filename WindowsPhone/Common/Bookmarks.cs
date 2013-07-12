using SQLite;
using System;
using System.Collections.Generic;

namespace QuranPhone.Common
{
    [Table("bookmarks")]
    public class Bookmarks
    {
        [Column("_ID"), PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Column("sura")]
        public int? Sura { get; set; }
        [Column("ayah")]
        public int? Ayah { get; set; }
        [Column("page")]
        public int Page { get; set; }
        [Ignore]
        public IList<Tags> Tags { get; set; }
        [Column("added_date")]
        public DateTime AddedDate { get; set; }
    }
}
