using System;
using System.Globalization;
using Quran.Core.Data;
using Quran.Core.Utils;

namespace Quran.Core.Common
{
    public class RepeatManager
    {
        public RepeatInfo RepeatInfo { get; private set; }

        public QuranAyah FirstAyah { get; private set; }

        public QuranAyah LastAyah { get; private set; }

        public int Counter { get; private set; }

        public RepeatManager(RepeatInfo info, QuranAyah firstAyah, int currentIteration)
        {
            if (firstAyah == null)
                throw new ArgumentNullException("firstAyah");

            RepeatInfo = info ?? new RepeatInfo();

            Counter = currentIteration;

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
            else if (RepeatInfo.RepeatAmount == RepeatAmount.Rub)
            {
                var rub = QuranUtils.GetRub3FromAyah(FirstAyah.Surah, FirstAyah.Ayah);
                FirstAyah = QuranUtils.GetRub3FirstAyah(rub);
                LastAyah = QuranUtils.GetRub3LastAyah(rub);
            }
            else if (RepeatInfo.RepeatAmount == RepeatAmount.Juz)
            {
                var juz = QuranUtils.GetJuzFromAyah(FirstAyah.Surah, FirstAyah.Ayah);
                FirstAyah = QuranUtils.GetJuzFirstAyah(juz);
                LastAyah = QuranUtils.GetJuzLastAyah(juz);
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
            Counter++;
        }

        public void DecrementCounter()
        {
            Counter--;
        }

        public bool ShouldRepeat()
        {
            if (RepeatInfo.RepeatAmount == RepeatAmount.None)
                return false;
            if (Counter >= RepeatInfo.RepeatCount)
                return false;
            else
                return true;
        }
    }
}
