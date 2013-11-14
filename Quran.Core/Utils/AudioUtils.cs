using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Cirrious.CrossCore;
using Cirrious.CrossCore.Platform;
using Quran.Core.Data;
using Quran.Core.Common;
using Quran.Core.Data;
using Quran.Core.Properties;

namespace Quran.Core.Utils
{
    public enum AudioDownloadAmount
    {
        Page = 1,
        Sura = 2,
        Juz = 3
    }

    public static class AudioUtils
    {
        public const string AudioExtension = ".mp3";
        private static readonly RecitersDatabaseHandler databaseHandler = new RecitersDatabaseHandler();

        #region Public Methods

        public static int GetReciterIdByName(string name)
        {
            var reciter = databaseHandler.GetReciter(name);
            if (reciter == null)
                return -1;
            return reciter.Id;
        }

        public static IEnumerable<ReciterItem> GetReciterItems()
        {
            return databaseHandler.GetAllReciters();
        }

        public static ReciterItem GetReciterById(int position)
        {
            return databaseHandler.GetReciter(position);
        }

        public static bool ShouldDownloadGaplessDatabase(AudioRequest request)
        {
            if (!request.Reciter.IsGapless)
            {
                return false;
            }
            string dbPath = request.Reciter.GaplessDatabasePath;
            if (string.IsNullOrEmpty(dbPath))
            {
                return false;
            }

            return !FileUtils.FileExists(dbPath);
        }

        public static QuranAyah GetLastAyahToPlay(QuranAyah startAyah, AudioDownloadAmount mode)
        {
            switch (mode)
            {
                case AudioDownloadAmount.Page:
                    return GetLastAyahToPlayForPage(startAyah);
                case AudioDownloadAmount.Sura:
                    return GetLastAyahToPlayForSura(startAyah);
                case AudioDownloadAmount.Juz:
                    return GetLastAyahToPlayForJuz(startAyah);
                default:
                    return GetLastAyahToPlayForPage(startAyah);
            }
        }

        private static QuranAyah GetLastAyahToPlayForSura(QuranAyah startAyah)
        {
            int sura = startAyah.Sura;
            int lastAyah = QuranInfo.GetSuraNumberOfAyah(sura);
            return new QuranAyah(sura, lastAyah);
        }

        private static QuranAyah GetLastAyahToPlayForPage(QuranAyah startAyah)
        {
            var page = QuranInfo.GetPageFromSuraAyah(startAyah.Sura, startAyah.Ayah);
            if (page == -1)
                return null;

            int[] pageBounds = QuranInfo.GetPageBounds(page);

            return new QuranAyah(pageBounds[2], pageBounds[3]);
        }

        private static QuranAyah GetLastAyahToPlayForJuz(QuranAyah startAyah)
        {
            var juz = QuranInfo.GetJuzFromAyah(startAyah.Sura, startAyah.Ayah);
            // If last juz - return last verse
            if (juz == Constants.JUZ2_COUNT)
                return new QuranAyah(Constants.SURA_LAST, 6);

            int[] endJuz = QuranInfo.QUARTERS[juz * 8];

            return new QuranAyah(endJuz[0], endJuz[1] - 1);
        }

        public static bool ShouldDownloadBismillah(AudioRequest request)
        {
            if (request.Reciter.IsGapless)
            {
                return false;
            }
            string baseDirectory = request.Reciter.LocalPath;
            if (!string.IsNullOrEmpty(baseDirectory))
            {
                if (FileUtils.DirectoryExists(baseDirectory))
                {
                    string filename = string.Format("1\\1{0}", AudioExtension);
                    if (FileUtils.FileExists(FileUtils.Combine(baseDirectory, filename)))
                    {
                        return false;
                    }
                }
                else
                {
                    FileUtils.MakeDirectory(baseDirectory);
                }
            }

            return DoesRequireBismillah(request);
        }

        public static bool HaveSuraAyahForQari(string baseDir, int sura, int ayah)
        {
            string filename = FileUtils.Combine(baseDir, sura + "\\" + ayah + AudioExtension);
            return FileUtils.FileExists(filename);
        }

        public static bool DoesRequireBismillah(AudioRequest request)
        {
            QuranAyah minAyah = request.MinAyah;
            int startSura = minAyah.Sura;
            int startAyah = minAyah.Ayah;

            QuranAyah maxAyah = request.MaxAyah;
            int endSura = maxAyah.Sura;
            int endAyah = maxAyah.Ayah;

            for (int i = startSura; i <= endSura; i++)
            {
                int lastAyah = QuranInfo.GetSuraNumberOfAyah(i);
                if (i == endSura)
                {
                    lastAyah = endAyah;
                }
                int firstAyah = 1;
                if (i == startSura)
                {
                    firstAyah = startAyah;
                }

                for (int j = firstAyah; j <= lastAyah; j++)
                {
                    if (j == 1 && i != Constants.SURA_FIRST && i != Constants.SURA_TAWBA)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool HaveAllFiles(AudioRequest request)
        {
            string baseDirectory = request.Reciter.LocalPath;
            if (string.IsNullOrEmpty(baseDirectory))
                return false;

            if (!FileUtils.DirectoryExists(baseDirectory))
                return false;

            foreach (var verse in QuranInfo.GetAllAyah(request.MinAyah, request.MaxAyah))
            {
                var filename = GetLocalPathForAyah(verse, request.Reciter);
                if (!FileUtils.FileExists(filename.ToString()))
                {
                    return false;
                }
            }

            return true;
        }

        public static string GetLocalPathForAyah(QuranAyah ayah, ReciterItem reciter)
        {
            string fileName;
            if (reciter.IsGapless)
                fileName = string.Format(reciter.GetFilePattern(), ayah.Sura);
            else
                fileName = string.Format(reciter.GetFilePattern(), ayah.Sura, ayah.Ayah);

            return PathHelper.Combine(reciter.LocalPath, fileName);
        }

        public static string GetServerPathForAyah(QuranAyah ayah, ReciterItem reciter)
        {
            string fileName;
            if (reciter.IsGapless)
                fileName = string.Format(reciter.GetFilePattern(), ayah.Sura);
            else
                fileName = string.Format(reciter.GetFilePattern(), ayah.Sura, ayah.Ayah);

            return PathHelper.Combine(reciter.ServerUrl, fileName);
        }

        public static async Task<bool> DownloadRange(AudioRequest request)
        {
            var ayahToDownload = QuranInfo.GetAllAyah(request.MinAyah, request.MaxAyah);
            var filesToDownload = new List<string>();
            bool result = true;

            foreach (var ayah in ayahToDownload)
            {
                filesToDownload.Add(GetServerPathForAyah(ayah, request.Reciter));
            }

            result = await QuranApp.DetailsViewModel.ActiveDownload.DownloadMultiple(filesToDownload.ToArray(),
                        request.Reciter.LocalPath, AppResources.loading_audio);

            if (result)
            {
                // attempt to download bismillah if it doesn't exist
                var bismillaFile = GetLocalPathForAyah(new QuranAyah(1, 1), request.Reciter);
                if (!FileUtils.FileExists(bismillaFile))
                {
                    QuranApp.NativeProvider.Log("bismillah doesn't exist, downloading...");
                    result = await FileUtils.DownloadFileFromWebAsync(GetServerPathForAyah(new QuranAyah(1, 1), request.Reciter), 
                        request.Reciter.LocalPath);
                }
            }

            return result;
        }

        public static async Task<bool> DownloadGaplessRange(string urlString, string destination, QuranAyah fromAyah, QuranAyah toAyah)
        {
            var result = true;
            for (int i = fromAyah.Sura; i <= toAyah.Sura; i++)
            {
                string serverUrl = string.Format(CultureInfo.InvariantCulture, urlString, i);
                var localUrl = destination + "/";
                QuranApp.NativeProvider.Log("gapless asking to download " + serverUrl + " to " + localUrl);

                result = await QuranApp.DetailsViewModel.ActiveDownload.Download(serverUrl, localUrl, AppResources.loading_audio);
                if (!result)
                    break;
            }
            return result;
        }

        //public static void PlayGapless(string localPath, QuranAyah ayah, ReciterItem reciter)
        //{
        //    var fileName = string.Format("{0:000}.mp3", ayah.Sura);
        //    var fullPath = PathHelper.Combine(localPath, fileName);
        //    var fullPathAsUri = new Uri(fullPath, UriKind.Relative);
        //    var track = QuranApp.NativeProvider.AudioProvider.GetTrack();
        //    var title = QuranInfo.GetSuraAyahString(ayah.Sura, ayah.Ayah);
        //    if (track == null || track.Source != fullPathAsUri)
        //    {
        //        QuranApp.NativeProvider.AudioProvider.SetTrack(fullPathAsUri, title, reciter.Name, "Quran", null, null);
        //    }
        //    QuranApp.NativeProvider.AudioProvider.Play();
        //}

        //public static void PlayNonGapless(string localPath, QuranAyah ayah, ReciterItem reciter)
        //{
        //    var fileName = string.Format("{0:000}{1:000}.mp3", ayah.Sura, ayah.Ayah);
        //    var fullPath = PathHelper.Combine(localPath, fileName);
        //    var fullPathAsUri = new Uri(fullPath, UriKind.Relative);
        //    var track = QuranApp.NativeProvider.AudioProvider.GetTrack();
        //    var title = QuranInfo.GetSuraAyahString(ayah.Sura, ayah.Ayah);
        //    if (track == null || track.Source != fullPathAsUri)
        //    {
        //        QuranApp.NativeProvider.AudioProvider.SetTrack(fullPathAsUri, title, reciter.Name, "Quran", null, null);
        //    }
        //}

        public static string GetFilePattern(this ReciterItem reciter)
        {
            if (reciter.IsGapless)
            {
                return "{0:000}" + AudioExtension;
            }
            else
            {
                return "{0:000}{1:000}" + AudioExtension;
            }
        }
        #endregion
    }
}
