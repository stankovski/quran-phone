using System;
using Quran.Core.Common;
using Quran.Core.Data;

namespace Quran.Core.Utils
{
    public class AudioRequest
    {
        public AudioRequest(int reciterId, QuranAyah verse, LookAheadAmount lookAheadAmount)
        {
            if (verse == null)
                throw new ArgumentNullException("verse");

            if (verse == null || verse.Sura < 1 || verse.Sura > 114)
                throw new ArgumentException("verse");

            this.Reciter = AudioUtils.GetReciterById(reciterId);
            this.LookAheadAmount = lookAheadAmount;
            this.MinAyah = verse;
            this.MaxAyah = AudioUtils.GetLastAyahToPlay(verse, lookAheadAmount);
            this.CurrentAyah = verse;
            this.RepeatInfo = new RepeatInfo();
        }

        /// <summary>
        /// AudioRequest from a formatted string
        /// </summary>
        /// <param name="formattedString">LookAheadAmount/reciterId/surah/ayah</param>
        public AudioRequest(string formattedString)
        {
            if (string.IsNullOrEmpty(formattedString))
                throw new ArgumentNullException("formattedString");

            var splitString = formattedString.Split('/');
            if (splitString.Length != 4)
                throw new ArgumentException("formattedString");

            this.LookAheadAmount = (LookAheadAmount)Enum.Parse(typeof(LookAheadAmount), splitString[0]);
            this.Reciter = AudioUtils.GetReciterById(int.Parse(splitString[1]));
            var verse = new QuranAyah(int.Parse(splitString[2]), int.Parse(splitString[3]));

            this.MinAyah = verse;
            this.MaxAyah = verse;
            this.CurrentAyah = verse;
            this.RepeatInfo = new RepeatInfo();
        }

        public ReciterItem Reciter { get; private set; }

        public LookAheadAmount LookAheadAmount { get; set; }

        public QuranAyah CurrentAyah { get; set; }

        public RepeatInfo RepeatInfo { get; set; }
        
        public QuranAyah MinAyah { get; set; }

        public QuranAyah MaxAyah { get; set; }

        public void GotoNextAyah()
        {
            CurrentAyah = QuranInfo.GetNextAyah(CurrentAyah);
        }

        public void GotoPreviousAyah()
        {
            CurrentAyah = QuranInfo.GetPreviousAyah(CurrentAyah);
        }

        public override string ToString()
        {
            return string.Format("{0}/{1}/{2}/{3}", LookAheadAmount, Reciter.Id, CurrentAyah.Sura, CurrentAyah.Ayah);
        }
    }
}
