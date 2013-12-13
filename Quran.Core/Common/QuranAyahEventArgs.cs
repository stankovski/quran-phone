using System;

namespace Quran.Core.Common
{
    public class QuranAyahEventArgs : EventArgs
    {
        public QuranAyahEventArgs(QuranAyah ayah)
        {
            QuranAyah = ayah;
        }

        public QuranAyah QuranAyah { get; set; }
    }
}
