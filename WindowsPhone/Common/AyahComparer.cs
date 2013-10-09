using System.Collections.Generic;

namespace QuranPhone.Common
{
    public class AyahComparer : IComparer<QuranAyah>
    {
        public int Compare(QuranAyah x, QuranAyah y)
        {
            if (x.Ayah == y.Ayah && x.Sura == y.Sura)
            {
                return 0;
            }
            if (x.Sura < y.Sura)
            {
                return -1;
            }
            if (x.Sura > y.Sura)
            {
                return 1;
            }
            if (x.Sura == y.Sura && x.Ayah < y.Ayah)
            {
                return -1;
            }
            return 1;
        }
    }
}