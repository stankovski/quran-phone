using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cirrious.MvvmCross.Plugins.Sqlite;

namespace Quran.Core.Common
{
    [Table("timings")]
    public class Timings
    {
        [Column("sura"), PrimaryKey]
        public int Surah { get; set; }
        [Column("ayah"), PrimaryKey]
        public int Ayah { get; set; }
        [Column("time")]
        public int Time { get; set; }
    }
}
