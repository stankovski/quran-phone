using System;
using QuranPhone.SQLite;

namespace QuranPhone.Common
{
    [Table("bookmark_tag")]
    public class BookmarkTags
    {
        [Column("_ID"), PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Column("bookmark_id"), Indexed]
        public int BookmarkId { get; set; }

        [Column("tag_id"), Indexed]
        public int TagId { get; set; }

        [Column("added_date")]
        public DateTime AddedDate { get; set; }
    }
}