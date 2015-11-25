using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quran.Core.Data
{
    public class NavigationData
    {
        public int? Page { get; set; }
        public int? Surah { get; set; }
        public int? Ayah { get; set; }
    }
}
