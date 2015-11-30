using System;
using System.Collections.Generic;
using System.Globalization;
using SQLite.Net.Attributes;

namespace Quran.Core.Common
{
    [Table("verses")]
    public class QuranAyah
    {
        [Column("sura")]
        public int Surah { get; set; }
        [Column("ayah")]
        public int Ayah { get; set; }
        [Column("text")]
        public string Text { get; set; }
        [Ignore]
        public string Translation { get; set; }

        public QuranAyah() { }

        public QuranAyah(int surah, int ayah)
        {
            Surah = surah;
            Ayah = ayah;
        }

        public static bool operator >(QuranAyah a1, QuranAyah a2)
        {
            if (a1.Surah > a2.Surah)
                return true;
            else if (a1.Surah == a2.Surah && a1.Ayah > a2.Ayah)
                return true;
            else
                return false;
        }

        public static bool operator >=(QuranAyah a1, QuranAyah a2)
        {
            if (a1 > a2 || a1.Equals(a2))
                return true;
            else
                return false;
        }

        public static bool operator <(QuranAyah a1, QuranAyah a2)
        {
            if (a1.Surah < a2.Surah)
                return true;
            else if (a1.Surah == a2.Surah && a1.Ayah < a2.Ayah)
                return true;
            else
                return false;
        }

        public static bool operator <=(QuranAyah a1, QuranAyah a2)
        {
            if (a1 < a2 || a1.Equals(a2))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Creates QuranAyah from string
        /// </summary>
        /// <param name="text">Text in the surah:ayah format</param>
        public static QuranAyah FromString(string text)
        {
            try
            {
                var splitText = text.Split(':');
                return new QuranAyah(int.Parse(splitText[0]), int.Parse(splitText[1]));
            }
            catch
            {
                throw new ArgumentException("text");
            }
        }

        public QuranAyah(QuranAyah ayah)
        {
            Surah = ayah.Surah;
            Ayah = ayah.Ayah;
            Text = ayah.Text;
            Translation = ayah.Translation;
        }

        public override string ToString()
        {
            return string.Format("{0}:{1}", Surah, Ayah, CultureInfo.InvariantCulture);
        }

        public override bool Equals(object obj)
        {
            var anotherAyah = obj as QuranAyah;

            if (anotherAyah == null)
                return false;

            return anotherAyah.Ayah == this.Ayah && anotherAyah.Surah == this.Surah;
        }

        public override int GetHashCode()
        {
            return this.Ayah.GetHashCode() + this.Surah.GetHashCode();
        }

        public QuranAyah Clone()
        {
            QuranAyah ayah = new QuranAyah();
            ayah.Ayah = this.Ayah;
            ayah.Surah = this.Surah;
            ayah.Text = this.Text;
            ayah.Translation = this.Translation;
            return ayah;
        }
    }

    [Table("arabic_text")]
    public class ArabicAyah : QuranAyah
    {
        public ArabicAyah() { }

        public ArabicAyah(int surah, int ayah) : base(surah, ayah)
        {}
    }

    public class AyahComparer : IComparer<QuranAyah>
    {
        public int Compare(QuranAyah x, QuranAyah y)
        {
            if (x.Ayah == y.Ayah && x.Surah == y.Surah)
                return 0;
            else if (x.Surah < y.Surah)
                return -1;
            else if (x.Surah > y.Surah)
                return 1;
            else if (x.Surah == y.Surah && x.Ayah < y.Ayah)
                return -1;
            else
                return 1;
        }
    }
}
