using System.Collections.Generic;
using SQLite;

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

        public override string ToString()
        {
            return string.Format("{0}:{1}", Sura, Ayah);
        }
    }

    [Table("arabic_text")]
    public class ArabicAyah : QuranAyah
    {
        public ArabicAyah() { }

        public ArabicAyah(int sura, int ayah) : base(sura, ayah)
        {}
    }

    public class AyahComparer : IComparer<QuranAyah>
    {
        public int Compare(QuranAyah x, QuranAyah y)
        {
            if (x.Ayah == y.Ayah && x.Sura == y.Sura)
                return 0;
            else if (x.Sura < y.Sura)
                return -1;
            else if (x.Sura > y.Sura)
                return 1;
            else if (x.Sura == y.Sura && x.Ayah < y.Ayah)
                return -1;
            else
                return 1;
        }
    }
}
