using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuranPhone.Common
{
    [Table("verses")]
    public class QuranAyah
    {
        [Column("sura")]
        public int Sura { get; set; }
        [Column("ayah")]
        public int Ayah { get; set; }
        [Column("text")]
        public string Text { get; set; }
        [Ignore]
        public string Translation { get; set; }

        public QuranAyah() { }

        public QuranAyah(int sura, int ayah)
        {
            Sura = sura;
            Ayah = ayah;
        }
    }
}
