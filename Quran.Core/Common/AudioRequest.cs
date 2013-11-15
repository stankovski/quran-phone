using System;
using Quran.Core.Utils;

namespace Quran.Core.Common
{
    public class AudioRequest
    {
        public AudioRequest(int reciterId, QuranAyah verse, AudioDownloadAmount audioDownloadAmount)
        {
            if (verse == null)
                throw new ArgumentNullException("verse");

            if (verse == null || verse.Sura < 1 || verse.Sura > 114)
                throw new ArgumentException("verse");

            this.Reciter = AudioUtils.GetReciterById(reciterId);
            this.AudioDownloadAmount = audioDownloadAmount;
            this.MinAyah = verse;
            this.MaxAyah = AudioUtils.GetLastAyahToPlay(verse, audioDownloadAmount);
            this.CurrentAyah = verse;
            this.RepeatInfo = new RepeatInfo();
        }

        /// <summary>
        /// AudioRequest from a formatted string
        /// </summary>
        /// <param name="formattedString">AudioDownloadAmount/reciterId/surah/ayah</param>
        public AudioRequest(string formattedString)
        {
            if (string.IsNullOrEmpty(formattedString))
                throw new ArgumentNullException("formattedString");

            var splitString = formattedString.Split('/');
            if (splitString.Length != 4)
                throw new ArgumentException("formattedString");

            this.AudioDownloadAmount = (AudioDownloadAmount)Enum.Parse(typeof(AudioDownloadAmount), splitString[0]);
            this.Reciter = AudioUtils.GetReciterById(int.Parse(splitString[1]));
            var verse = new QuranAyah(int.Parse(splitString[2]), int.Parse(splitString[3]));

            this.MinAyah = verse;
            this.MaxAyah = verse;
            this.CurrentAyah = verse;
            this.RepeatInfo = new RepeatInfo();
        }

        public ReciterItem Reciter { get; private set; }

        public AudioDownloadAmount AudioDownloadAmount { get; set; }

        public QuranAyah CurrentAyah { get; set; }

        public RepeatInfo RepeatInfo { get; set; }
        
        public QuranAyah MinAyah { get; set; }

        public QuranAyah MaxAyah { get; set; }

        public void GotoNextAyah()
        {
            CurrentAyah = QuranUtils.GetNextAyah(CurrentAyah, true);
        }

        public void GotoPreviousAyah()
        {
            CurrentAyah = QuranUtils.GetPreviousAyah(CurrentAyah, true);
        }

        public override string ToString()
        {
            return string.Format("{0}/{1}/{2}/{3}", AudioDownloadAmount, Reciter.Id, CurrentAyah.Sura, CurrentAyah.Ayah);
        }
    }
}
