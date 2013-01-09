using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuranPhone.Common
{
    public class QuranAyah
    {
        private static long serialVersionUID = 1L;

        public int Sura { get; set; }
        public int Ayah { get; set; }
        public string Text { get; set; }
        public string Translation { get; set; }

        public QuranAyah() { }

        public QuranAyah(int sura, int ayah)
        {
            Sura = sura;
            Ayah = ayah;
        }
    }
}
