using System;
using Quran.Core.Utils;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Quran.Core.Data;

namespace Quran.Core.Common
{
    [DataContract]
    public class QuranAudioTrack
    {
        public QuranAudioTrack() { }

        public QuranAudioTrack(int reciterId, QuranAyah verse, bool isStreaming, AudioDownloadAmount downloadAmount, ScreenInfo qsi)
        {
            if (verse == null)
            {
                throw new ArgumentNullException(nameof(verse));
            }

            if (verse == null || verse.Surah < 1 || verse.Surah > 114)
            {
                throw new ArgumentException("Invalid Surah number.", nameof(verse));
            }

            this.ReciterId = reciterId;
            this.Surah = verse.Surah;
            this.Ayah = verse.Ayah;
            this.IsStreaming = isStreaming;
            this.DownloadAmount = downloadAmount;
            this.ScreenInfo = qsi;
        }
        
        [DataMember]
        public int ReciterId { get; set; }

        [DataMember]
        public int Surah { get; set; }

        [DataMember]
        public int Ayah { get; set; }

        [DataMember]
        public bool IsStreaming { get; set; }

        [DataMember]
        public AudioDownloadAmount DownloadAmount { get; set; }

        [DataMember]
        public ScreenInfo ScreenInfo { get; set; }

        public QuranAudioTrack GetNextInSurah()
        {
            var newTrack = GetNext();
            if (newTrack.Surah != this.Surah)
            {
                return null;
            }
            return newTrack;
        }

        public QuranAudioTrack GetNext()
        {
            return new QuranAudioTrack(ReciterId, 
                QuranUtils.GetNextAyah(GetQuranAyah(), true), 
                IsStreaming, DownloadAmount, ScreenInfo);
        }

        public QuranAudioTrack GetPreviousInSurah()
        {
            var newTrack = GetPrevious();
            if (newTrack.Surah != this.Surah)
            {
                return null;
            }
            return newTrack;
        }

        public QuranAudioTrack GetPrevious()
        {
            return new QuranAudioTrack(ReciterId, 
                QuranUtils.GetPreviousAyah(GetQuranAyah(), true), 
                IsStreaming, DownloadAmount, ScreenInfo);
        }

        public QuranAudioTrack GetFirstAyah()
        {
            int firstAyah = Surah == Constants.SURA_FIRST || Surah == Constants.SURA_TAWBA ? 1 : 0;
            return new QuranAudioTrack(ReciterId,
                new QuranAyah(Surah, firstAyah),
                IsStreaming, DownloadAmount, ScreenInfo);
        }

        public QuranAudioTrack GetLastAyah()
        {
            return new QuranAudioTrack(ReciterId,
                new QuranAyah(Surah, QuranUtils.GetSurahNumberOfAyah(Surah)),
                IsStreaming, DownloadAmount, ScreenInfo);
        }

        public ReciterItem GetReciter()
        {
            return AudioUtils.GetReciterById(ReciterId);
        }
        public QuranAyah GetQuranAyah()
        {
            return new QuranAyah(Surah, Ayah);
        }

        public void UpdateQuranAyah(QuranAyah ayah)
        {
            Surah = ayah.Surah;
            Ayah = ayah.Ayah;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static QuranAudioTrack FromString(string json)
        {
            return JsonConvert.DeserializeObject<QuranAudioTrack>(json);
        }
    }
}
