using System;
using System.Globalization;
using System.Threading.Tasks;
using Quran.Core.Data;
using Quran.Core.Common;
using Quran.Core.Data;
using Quran.Core.Properties;

namespace Quran.Core.Utils
{
    public class AudioRequest
    {
        private string mBaseUrl = null;
        private string mGaplessDatabasePath = null;

        // where we started from
        private int mStartSura = 0;
        private int mStartAyah = 0;
        private int mAyahsInThisSura = 0;

        // min and max sura/ayah
        private int mMinSura = 0;
        private int mMinAyah = 0;
        private int mMaxSura = 0;
        private int mMaxAyah = 0;

        // what we're currently playing
        private int mCurrentSura = 0;
        private int mCurrentAyah = 0;

        // did we just play the basmallah?
        private bool mJustPlayedBasmallah = false;

        // repeat information
        private RepeatInfo mRepeatInfo;

        private int mQariId = -1;
        private string mLocalDirectoryPath = null;

        public AudioRequest(string baseUrl, QuranAyah verse)
        {
            mBaseUrl = baseUrl;
            mStartSura = verse.Sura;
            mStartAyah = verse.Ayah;

            if (mStartSura < 1 || mStartSura > 114 || mStartAyah < 1)
            {
                throw new ArgumentException();
            }

            mCurrentSura = mStartSura;
            mCurrentAyah = mStartAyah;
            mAyahsInThisSura = QuranInfo.SURA_NUM_AYAHS[mCurrentSura - 1];

            mRepeatInfo = new RepeatInfo(0);
            mRepeatInfo.setCurrentVerse(mCurrentSura, mCurrentAyah);
        }

        public AudioRequest(string baseUrl, QuranAyah verse,
                                    int qariId, string localPath)
            : this(baseUrl, verse)
        {
            mQariId = qariId;
            mLocalDirectoryPath = localPath;
        }

        public int GetQariId() { return mQariId; }
        
        public string GetLocalPath() { return mLocalDirectoryPath; }

        public void SetGaplessDatabaseFilePath(string databaseFile)
        {
            mGaplessDatabasePath = databaseFile;
        }

        public string GetGaplessDatabaseFilePath()
        {
            return mGaplessDatabasePath;
        }

        public bool IsGapless()
        {
            return mGaplessDatabasePath != null;
        }

        public void SetRepeatInfo(RepeatInfo repeatInfo)
        {
            if (repeatInfo == null)
            {
                repeatInfo = new RepeatInfo(0);
            }
            if (mRepeatInfo != null)
            {
                // just update the repeat count for now
                mRepeatInfo.setRepeatCount(repeatInfo.getRepeatCount());
            }
            else { mRepeatInfo = repeatInfo; }
        }

        public QuranAyah SetCurrentAyah(int sura, int ayah)
        {
            if (mRepeatInfo.shouldRepeat())
            {
                mRepeatInfo.incrementRepeat();
                return mRepeatInfo.getCurrentAyah();
            }
            else
            {
                mCurrentSura = sura;
                mCurrentAyah = ayah;
                mRepeatInfo.setCurrentVerse(mCurrentSura, mCurrentAyah);
                return null;
            }
        }

        public string GetBaseUrl() { return mBaseUrl; }

        public void SetPlayBounds(QuranAyah minVerse, QuranAyah maxVerse)
        {
            mMinSura = minVerse.Sura;
            mMinAyah = minVerse.Ayah;
            mMaxSura = maxVerse.Sura;
            mMaxAyah = maxVerse.Ayah;
        }

        public void RemovePlayBounds()
        {
            mMinSura = 0;
            mMinAyah = 0;
            mMaxSura = 0;
            mMaxAyah = 0;
        }

        public QuranAyah GetMinAyah()
        {
            return new QuranAyah(mMinSura, mMinAyah);
        }

        public QuranAyah GetMaxAyah()
        {
            return new QuranAyah(mMaxSura, mMaxAyah);
        }

        public string GetUrl()
        {
            if ((mMaxSura > 0 && mCurrentSura > mMaxSura)
                  || (mMaxAyah > 0 && mCurrentAyah > mMaxAyah
                     && mCurrentSura >= mMaxSura)
                  || (mMinSura > 0 && mCurrentSura < mMinSura)
                  || (mMinAyah > 0 && mCurrentAyah < mMinAyah
                     && mCurrentSura <= mMinSura)
                  || mCurrentSura > 114
                  || mCurrentSura < 1)
            {
                return null;
            }

            if (IsGapless())
            {
                string url = string.Format(mBaseUrl, mCurrentSura);
                return url;
            }

            int sura = mCurrentSura;
            int ayah = mCurrentAyah;
            if (ayah == 1 && sura != 1 && sura != 9 && !mJustPlayedBasmallah)
            {
                mJustPlayedBasmallah = true;
                sura = 1;
                ayah = 1;
            }
            else { mJustPlayedBasmallah = false; }

            if (mJustPlayedBasmallah)
            {
                // really if "about to play" bismillah...
                if (!HaveSuraAyah(mCurrentSura, mCurrentAyah))
                {
                    // if we don't have the first ayah, don't play basmallah
                    return null;
                }
            }

            return string.Format(mBaseUrl, sura, ayah);
        }

        public bool HaveSuraAyah(int sura, int ayah)
        {
            // for streaming, we (theoretically) always "have" the sura and ayah
            return true;
        }

        public string GetTitle()
        {
            return QuranInfo.GetSuraAyahString(mCurrentSura, mCurrentAyah);
        }

        public int GetCurrentSura()
        {
            return mCurrentSura;
        }

        public int GetCurrentAyah()
        {
            return mCurrentAyah;
        }

        public void GotoNextAyah(bool force)
        {
            // don't go to next ayah if we haven't played basmallah yet
            if (mJustPlayedBasmallah) { return; }
            if (!force && mRepeatInfo.shouldRepeat())
            {
                mRepeatInfo.incrementRepeat();
                if (mCurrentAyah == 1 && mCurrentSura != 1 && mCurrentSura != 9)
                {
                    mJustPlayedBasmallah = true;
                }
                return;
            }

            mCurrentAyah++;
            if (mAyahsInThisSura < mCurrentAyah)
            {
                mCurrentAyah = 1;
                mCurrentSura++;
                if (mCurrentSura <= 114)
                {
                    mAyahsInThisSura = QuranInfo.SURA_NUM_AYAHS[mCurrentSura - 1];
                    mRepeatInfo.setCurrentVerse(mCurrentSura, mCurrentAyah);
                }
            }
            else { mRepeatInfo.setCurrentVerse(mCurrentSura, mCurrentAyah); }
        }

        public void GotoPreviousAyah()
        {
            mCurrentAyah--;
            if (mCurrentAyah < 1)
            {
                mCurrentSura--;
                if (mCurrentSura > 0)
                {
                    mAyahsInThisSura = QuranInfo.SURA_NUM_AYAHS[mCurrentSura - 1];
                    mCurrentAyah = mAyahsInThisSura;
                }
            }
            else if (mCurrentAyah == 1 && !IsGapless())
            {
                mJustPlayedBasmallah = true;
            }

            if (mCurrentSura > 0 && mCurrentAyah > 0)
            {
                mRepeatInfo.setCurrentVerse(mCurrentSura, mCurrentAyah);
            }
        }

        public event EventHandler<AudioTransferEventArgs> TransferStatusChanged;

        public async Task<bool> DownloadRange(string urlString, string destination)
        {
            QuranFileUtils.MakeDirectory(destination);

            int totalAyahs = 0;
            int processedAyahs = 0;
            int startSura = mMinSura;
            int startAyah = mMinAyah;
            int endSura = mMaxSura;
            int endAyah = mMaxAyah;

            if (IsGapless())
            {
                for (int i = startSura; i <= endSura; i++)
                {
                    string serverUrl = string.Format(CultureInfo.InvariantCulture, urlString, i);
                    var localUrl = destination + "/";
                    QuranApp.NativeProvider.Log("gapless asking to download " + serverUrl + " to " + localUrl);

                    return
                        await
                            QuranApp.DetailsViewModel.ActiveDownload.Download(serverUrl, localUrl,
                                AppResources.loading_audio);
                }
            }
            else
            {
                if (startSura == endSura)
                {
                    totalAyahs = endAyah - startAyah + 1;
                }
                else
                {
                    // add the number ayahs from suras in between start and end
                    for (int i = startSura + 1; i < endSura; i++)
                    {
                        totalAyahs += QuranInfo.GetNumAyahs(i);
                    }

                    // add the number of ayahs from the start sura
                    totalAyahs += QuranInfo.GetNumAyahs(startSura) - startAyah + 1;

                    // add the number of ayahs from the last sura
                    totalAyahs += endAyah;
                }
            }

            QuranApp.NativeProvider.Log("downloadRange for " + totalAyahs + " between " +
                    startSura + ":" + startAyah + " to " + endSura + ":" +
                    endAyah + ", gaplessFlag: " + IsGapless());

            if (TransferStatusChanged != null)
                TransferStatusChanged(this, new AudioTransferEventArgs(totalAyahs, 0, false));

            // extension and filename template don't change
            string filename = PathHelper.GetFileName(urlString);
            int extLocation = filename.LastIndexOf('.');
            string extension = filename.Substring(extLocation);

            bool result = true;
            for (int i = startSura; i <= endSura; i++)
            {
                int lastAyah = QuranInfo.GetNumAyahs(i);
                if (i == endSura) { lastAyah = endAyah; }
                int firstAyah = 1;
                if (i == startSura) { firstAyah = startAyah; }

                string destDir = string.Empty;

                // same destination directory for ayahs within the same sura
                destDir = destination + "/" + i + "/";
                QuranFileUtils.MakeDirectory(destDir);

                for (int j = firstAyah; j <= lastAyah; j++)
                {
                    string url = string.Format(CultureInfo.InvariantCulture, urlString, i, j);
                    string destFile = j + extension;

                    result = await QuranFileUtils.DownloadFileFromWebAsync(url, PathHelper.Combine(destDir, destFile));

                    if (!result) { return false; }

                    if (TransferStatusChanged != null)
                        TransferStatusChanged(this, new AudioTransferEventArgs(totalAyahs, processedAyahs++, false));
                }
            }

            if (result)
            {
                // attempt to download basmallah if it doesn't exist
                string destDir = destination + "/" + 1 + "/";
                QuranFileUtils.MakeDirectory(destDir);

                if (!QuranFileUtils.FileExists(PathHelper.Combine(destDir, "1" + extension)))
                {
                    QuranApp.NativeProvider.Log("basmallah doesn't exist, downloading...");
                    string url = string.Format(CultureInfo.InvariantCulture, urlString, 1, 1);
                    string destFile = 1 + extension;

                    result = await QuranFileUtils.DownloadFileFromWebAsync(url, PathHelper.Combine(destDir, destFile));

                    if (!result) { return false; }
                }

                if (TransferStatusChanged != null)
                    TransferStatusChanged(this, new AudioTransferEventArgs(totalAyahs, totalAyahs, true));
            }

            return result;
        }

        public void Play()
        {
            if (IsGapless())
            {
                var fileName = string.Format("{0:000}.mp3", mCurrentSura);
                var path = PathHelper.Combine(GetLocalPath(), fileName);
                var localPath = new Uri(path, UriKind.Relative);
                var track = QuranApp.NativeProvider.AudioProvider.GetTrack();
                //if (track == null || track.Source != localPath)
                //{
                //    QuranApp.NativeProvider.AudioProvider.SetTrack(localPath, GetTitle(),
                //        AudioUtils.GetQariNameByPosition(GetQariId()), "Quran", null, null);
                //}
                QuranApp.NativeProvider.AudioProvider.Play();
            }
        }
    }
}
