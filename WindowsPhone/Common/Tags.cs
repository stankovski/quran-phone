using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuranPhone.Common
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
