using QuranPhone.SQLite;

namespace QuranPhone.Common
{
    [Table("verses")]
    public class QuranAyah
    {
        public QuranAyah() { }

        public QuranAyah(int sura, int ayah)
        {
            Sura = sura;
            Ayah = ayah;
        }

        [Column("sura")]
        public int Sura { get; set; }

        [Column("ayah")]
        public int Ayah { get; set; }

        [Column("text")]
        public string Text { get; set; }

        [Ignore]
        public string Translation { get; set; }

        public override string ToString()
        {
            return string.Format("{0}:{1}", Sura, Ayah);
        }
    }
}