using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuranPhone.Common
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
