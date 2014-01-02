using System;
using System.Collections.Generic;
using Quran.Core.Utils;
using System.Globalization;

namespace Quran.Core.Common
{
    public class AudioRequest
    {
        private RepeatManager repeatManager;

        public AudioRequest(int reciterId, QuranAyah verse, RepeatInfo repeat, AudioDownloadAmount audioDownloadAmount)
        {
            if (verse == null)
                throw new ArgumentNullException("verse");

            if (verse == null || verse.Surah < 1 || verse.Surah > 114)
                throw new ArgumentException("verse");

            this.Reciter = AudioUtils.GetReciterById(reciterId);
            this.AudioDownloadAmount = audioDownloadAmount;
            this.FromAyah = verse;
            this.CurrentAyah = verse;
            this.ToAyah = AudioUtils.GetLastAyahToPlay(verse, audioDownloadAmount);

            if (repeat != null)
            {
                this.RepeatInfo = repeat;
            }
            else
            {
                this.RepeatInfo = new RepeatInfo();
            }

            this.repeatManager = new RepeatManager(this.RepeatInfo, verse);
        }

        /// <summary>
        /// AudioRequest from a formatted string
        /// </summary>
        /// <param name="formattedString">[local|streaming]://reciterId?amount=AudioDownloadAmount&amp;currentAyah=1:2&amp;fromAyah=1:2&amp;to=2:1&amp;repeat=xxx</param>
        public AudioRequest(string formattedString)
        {
            if (string.IsNullOrEmpty(formattedString))
                throw new ArgumentNullException("formattedString");

            try
            {
                Uri patternAsUri = new Uri(formattedString);
                if (patternAsUri.Scheme.Equals("local", StringComparison.OrdinalIgnoreCase))
                    IsStreaming = false;
                else if (patternAsUri.Scheme.Equals("streaming", StringComparison.OrdinalIgnoreCase))
                    IsStreaming = true;
                else
                    throw new ArgumentException("scheme");

                this.Reciter = AudioUtils.GetReciterById(int.Parse(patternAsUri.Host));

                var splitQueryString = patternAsUri.Query.Split(new char[] { '?', '&' });
                foreach (var part in splitQueryString)
                {
                    var splitPart = part.Split('=');
                    if (splitPart[0].Equals("amount", StringComparison.OrdinalIgnoreCase))
                    {
                        this.AudioDownloadAmount =
                            (AudioDownloadAmount) Enum.Parse(typeof (AudioDownloadAmount), splitPart[1]);
                    }
                    else if (splitPart[0].Equals("currentAyah", StringComparison.OrdinalIgnoreCase))
                    {
                        this.CurrentAyah = QuranAyah.FromString(splitPart[1]);
                    }
                    else if (splitPart[0].Equals("fromAyah", StringComparison.OrdinalIgnoreCase))
                    {
                        this.FromAyah = QuranAyah.FromString(splitPart[1]);
                    }
                    else if (splitPart[0].Equals("toAyah", StringComparison.OrdinalIgnoreCase))
                    {
                        this.ToAyah = QuranAyah.FromString(splitPart[1]);
                    }
                    else if (splitPart[0].Equals("repeat", StringComparison.OrdinalIgnoreCase))
                    {
                        this.RepeatInfo = RepeatInfo.FromString(splitPart[1]);
                    }
                }

                if (this.CurrentAyah == null)
                    this.CurrentAyah = this.FromAyah;
            }
            catch
            {
                throw new ArgumentException("formattedString");
            }
        }

        public ReciterItem Reciter { get; private set; }

        public AudioDownloadAmount AudioDownloadAmount { get; set; }

        public QuranAyah CurrentAyah { get; set; }

        public RepeatInfo RepeatInfo { get; set; }

        public QuranAyah FromAyah { get; set; }

        public QuranAyah ToAyah { get; set; }

        public bool IsStreaming { get; set; }

        public void GotoNextAyah()
        {
            CurrentAyah = QuranUtils.GetNextAyah(CurrentAyah, true);
            if (repeatManager.ShouldRepeat())
            {
                if (CurrentAyah > repeatManager.LastAyah)
                {
                    CurrentAyah = repeatManager.FirstAyah;
                    repeatManager.IncrementCounter();
                }
            }
        }

        public void GotoPreviousAyah()
        {
            CurrentAyah = QuranUtils.GetPreviousAyah(CurrentAyah, true);
            if (repeatManager.ShouldRepeat() && !QuranUtils.IsBismillah(CurrentAyah))
            {
                if (CurrentAyah < repeatManager.FirstAyah)
                {
                    CurrentAyah = repeatManager.LastAyah;
                    repeatManager.DecrementCounter();
                }
            }
        }

        /// <summary>
        /// Returns request formatted in the URI format
        /// [local|streaming]://reciterId?amount=AudioDownloadAmount&amp;currentAyah=1:2&amp;fromAyah=1:2&amp;to=2:1&amp;repeat=xxx
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var queryBuilder = new List<string>();
            var schema = IsStreaming ? "streaming" : "local";
            queryBuilder.Add("amount=" + AudioDownloadAmount);
            if (CurrentAyah != null)
                queryBuilder.Add("currentAyah=" + CurrentAyah);
            if (FromAyah != null)
                queryBuilder.Add("fromAyah=" + FromAyah);
            if (ToAyah != null)
                queryBuilder.Add("toAyah=" + ToAyah);
            if (RepeatInfo != null)
                queryBuilder.Add("repeat=" + RepeatInfo);

            var uriBuilder = new UriBuilder
            {
                Scheme = schema,
                Host = Reciter.Id.ToString(CultureInfo.InvariantCulture),
                Query = string.Join("&", queryBuilder)
            };
            return uriBuilder.ToString();
        }
    }
}
