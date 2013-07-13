using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuranPhone.Common
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
