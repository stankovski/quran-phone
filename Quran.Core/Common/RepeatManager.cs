using System;
using System.Globalization;
using Quran.Core.Data;
using Quran.Core.Utils;

namespace Quran.Core.Common
{
    public class RepeatManager
    {
        private int counter;

        public RepeatInfo RepeatInfo { get; private set; }

        public QuranAyah FirstAyah { get; private set; }

        public QuranAyah LastAyah { get; private set; }

        public RepeatManager(RepeatInfo info, QuranAyah firstAyah)
        {
            if (firstAyah == null)
                throw new ArgumentNullException("firstAyah");

            RepeatInfo = info ?? new RepeatInfo();

            FirstAyah = firstAyah;
            GenerateLastAyah();
        }

        private void GenerateLastAyah()
        {
            if (RepeatInfo.RepeatAmount == RepeatAmount.None)
            {
                LastAyah = FirstAyah;
            }
            else if (RepeatInfo.RepeatAmount == RepeatAmount.OneAyah)
            {
                LastAyah = GetLastAyahFromAyahCount(FirstAyah, 1);
            }
            else if (RepeatInfo.RepeatAmount == RepeatAmount.ThreeAyah)
            {
                LastAyah = GetLastAyahFromAyahCount(FirstAyah, 3);
            }
            else if (RepeatInfo.RepeatAmount == RepeatAmount.FiveAyah)
            {
                LastAyah = GetLastAyahFromAyahCount(FirstAyah, 5);
            }
            else if (RepeatInfo.RepeatAmount == RepeatAmount.TenAyah)
            {
                LastAyah = GetLastAyahFromAyahCount(FirstAyah, 10);
            }
            else if (RepeatInfo.RepeatAmount == RepeatAmount.Page)
            {
                int page = QuranUtils.GetPageFromAyah(FirstAyah);
                int[] pageBounds = QuranUtils.GetPageBounds(page);
                FirstAyah = new QuranAyah(pageBounds[0], pageBounds[1]);
                LastAyah = new QuranAyah(pageBounds[2], pageBounds[3]);
            }
            else if (RepeatInfo.RepeatAmount == RepeatAmount.Surah)
            {
                int surah = FirstAyah.Surah;
                int lastAyah = QuranUtils.GetSurahNumberOfAyah(surah);
                FirstAyah = new QuranAyah(surah, 1);
                LastAyah = new QuranAyah(surah, lastAyah);
            }
            else if (RepeatInfo.RepeatAmount == RepeatAmount.Juz)
            {
                var juz = QuranUtils.GetJuzFromAyah(FirstAyah.Surah, FirstAyah.Ayah);
                int[] endJuz = QuranUtils.QUARTERS[juz * 8];
                // If last juz - return last verse
                if (juz == Constants.JUZ2_COUNT)
                {
                    LastAyah = new QuranAyah(Constants.SURA_LAST, 6);
                }
                else
                {
                    LastAyah = new QuranAyah(endJuz[0], endJuz[1] - 1);
                }

                use get juz first Ayah and Last ayah
            }
        }

        private static QuranAyah GetLastAyahFromAyahCount(QuranAyah firstAyah, int count)
        {
            var currentAyah = firstAyah;
            for (int i = 1; i < count; i++)
            {
                currentAyah = QuranUtils.GetNextAyah(currentAyah, false);
            }
            return currentAyah;
        }

        public void IncrementCounter()
        {
            counter++;
        }

        public void DecrementCounter()
        {
            counter++;
        }

        public bool ShouldRepeat()
        {
            if (RepeatInfo.RepeatAmount == RepeatAmount.None)
                return false;
            if (counter >= RepeatInfo.RepeatCount)
                return false;
            else
                return true;
        }
    }
}
