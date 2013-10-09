using QuranPhone.SQLite;

namespace QuranPhone.Common
{
    [Table("arabic_text")]
    public class ArabicAyah : QuranAyah
    {
        public ArabicAyah() {}

        public ArabicAyah(int sura, int ayah) : base(sura, ayah) {}
    }
}