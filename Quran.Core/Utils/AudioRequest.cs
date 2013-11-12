using System;
using Quran.Core.Common;
using Quran.Core.Data;

namespace Quran.Core.Utils
{
    public class AudioRequest
    {
        public AudioRequest(int reciterId, QuranAyah verse)
        {
            if (verse == null)
                throw new ArgumentNullException("verse");

            if (verse == null || verse.Sura < 1 || verse.Sura > 114)
                throw new ArgumentException("verse");

            this.Reciter = AudioUtils.GetReciterItem(reciterId);
            this.MinAyah = verse;
            this.MaxAyah = verse;
            this.CurrentAyah = verse;
            this.RepeatInfo = new RepeatInfo();
        }

        /// <summary>
        /// AudioRequest from a formatted string
        /// </summary>
        /// <param name="formattedString">reciterId/surah/ayah</param>
        public AudioRequest(string formattedString)
        {
            if (string.IsNullOrEmpty(formattedString))
                throw new ArgumentNullException("formattedString");

            var splitString = formattedString.Split('/');
            if (splitString.Length != 3)
                throw new ArgumentException("formattedString");

            this.Reciter = AudioUtils.GetReciterItem(int.Parse(splitString[0]));
            var verse = new QuranAyah(int.Parse(splitString[1]), int.Parse(splitString[2]));

            this.MinAyah = verse;
            this.MaxAyah = verse;
            this.CurrentAyah = verse;
            this.RepeatInfo = new RepeatInfo();
        }

        public ReciterItem Reciter { get; private set; }

        public QuranAyah CurrentAyah { get; set; }

        public RepeatInfo RepeatInfo { get; set; }
        
        public QuranAyah MinAyah { get; set; }

        public QuranAyah MaxAyah { get; set; }

        public void GotoNextAyah()
        {
            var currentSurahPages = QuranInfo.GetSuraNumberOfAyah(CurrentAyah.Sura);

            // Check if not the end of surah
            if (CurrentAyah.Ayah < currentSurahPages)
            {
                CurrentAyah.Ayah++;
            }
            else
            {
                // If the end of surah check if also the end of Quran
                if (CurrentAyah.Sura < Constants.SURA_LAST)
                {
                    CurrentAyah.Sura++;
                }
                else
                {
                    CurrentAyah.Sura = Constants.SURA_FIRST;
                }
                CurrentAyah.Ayah = 1;
            }
        }

        public void GotoPreviousAyah()
        {
            // Check if not the beginning of surah
            if (CurrentAyah.Ayah > 1)
            {
                CurrentAyah.Ayah--;
            }
            else
            {
                // If the beginning of surah check if also the beginning of Quran
                if (CurrentAyah.Sura > Constants.SURA_FIRST)
                {
                    CurrentAyah.Sura--;
                }
                else
                {
                    CurrentAyah.Sura = Constants.SURA_LAST;
                }
                CurrentAyah.Ayah = QuranInfo.GetSuraNumberOfAyah(CurrentAyah.Sura);
            }
        }

        public override string ToString()
        {
            return string.Format("{0}/{1}/{2}", Reciter.Id, CurrentAyah.Sura, CurrentAyah.Ayah);
        }
    }
}
