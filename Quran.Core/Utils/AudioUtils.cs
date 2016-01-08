using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Quran.Core.Data;
using Quran.Core.Common;
using Quran.Core.Properties;
using System.IO;

namespace Quran.Core.Utils
{
    public enum AudioDownloadAmount
    {
        Page = 1,
        Surah = 2,
        Juz = 3
    }

    public enum RepeatAmount
    {
        None,
        OneAyah,
        ThreeAyah,
        FiveAyah,
        TenAyah,
        Page,
        Surah,
        Rub,
        Juz
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

        public static async Task<bool> ShouldDownloadGaplessDatabase(QuranAudioTrack request)
        {
            if (!request.GetReciter().IsGapless)
            {
                return false;
            }
            string dbPath = request.GetReciter().GaplessDatabasePath;
            if (string.IsNullOrEmpty(dbPath))
            {
                return false;
            }

            return !await FileUtils.FileExists(dbPath);
        }

        public static QuranAyah GetLastAyahToPlay(QuranAyah startAyah, AudioDownloadAmount mode)
        {
            switch (mode)
            {
                case AudioDownloadAmount.Page:
                    return GetLastAyahToPlayForPage(startAyah);
                case AudioDownloadAmount.Surah:
                    return GetLastAyahToPlayForSura(startAyah);
                case AudioDownloadAmount.Juz:
                    return GetLastAyahToPlayForJuz(startAyah);
                default:
                    return GetLastAyahToPlayForPage(startAyah);
            }
        }

        private static QuranAyah GetLastAyahToPlayForSura(QuranAyah startAyah)
        {
            int surah = startAyah.Surah;
            int lastAyah = QuranUtils.GetSurahNumberOfAyah(surah);
            return new QuranAyah(surah, lastAyah);
        }

        private static QuranAyah GetLastAyahToPlayForPage(QuranAyah startAyah)
        {
            var page = QuranUtils.GetPageFromAyah(startAyah.Surah, startAyah.Ayah);
            if (page == -1)
                return null;

            int[] pageBounds = QuranUtils.GetPageBounds(page);

            return new QuranAyah(pageBounds[2], pageBounds[3]);
        }

        private static QuranAyah GetLastAyahToPlayForJuz(QuranAyah startAyah)
        {
            var juz = QuranUtils.GetJuzFromAyah(startAyah.Surah, startAyah.Ayah);
            return QuranUtils.GetJuzLastAyah(juz);
        }

        public static async Task<bool> ShouldDownloadBismillah(AudioRequest request)
        {
            if (request.Reciter.IsGapless)
            {
                return false;
            }
            string baseDirectory = request.Reciter.LocalPath;
            if (!string.IsNullOrEmpty(baseDirectory))
            {
                if (await FileUtils.DirectoryExists(baseDirectory))
                {
                    string filename = string.Format("1\\1{0}", AudioExtension);
                    if (await FileUtils.FileExists(Path.Combine(baseDirectory, filename)))
                    {
                        return false;
                    }
                }
                else
                {
                    await FileUtils.EnsureDirectoryExists(baseDirectory);
                }
            }

            return DoesRequireBismillah(request);
        }

        public static async Task<bool> HaveSuraAyahForQari(string baseDir, int surah, int ayah)
        {
            string filename = Path.Combine(baseDir, surah + "\\" + ayah + AudioExtension);
            return await FileUtils.FileExists(filename);
        }

        public static bool DoesRequireBismillah(AudioRequest request)
        {
            QuranAyah minAyah = request.FromAyah;
            int startSura = minAyah.Surah;
            int startAyah = minAyah.Ayah;

            QuranAyah maxAyah = request.ToAyah;
            int endSura = maxAyah.Surah;
            int endAyah = maxAyah.Ayah;

            for (int i = startSura; i <= endSura; i++)
            {
                int lastAyah = QuranUtils.GetSurahNumberOfAyah(i);
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

        public static async Task<bool> HaveAllFiles(QuranAudioTrack request)
        {
            string baseDirectory = request.GetReciter().LocalPath;
            if (string.IsNullOrEmpty(baseDirectory))
                return false;

            if (!await FileUtils.DirectoryExists(baseDirectory))
                return false;

            var track = request.GetFirstAyah();
            while (track != null)
            {
                var filename = GetLocalPathForAyah(track.GetQuranAyah(), request.GetReciter());
                if (!await FileUtils.FileExists(filename.ToString()))
                {
                    return false;
                }
                track = track.GetNextInSurah();
            }
            
            return true;
        }

        public static string GetLocalPathForAyah(QuranAyah ayah, ReciterItem reciter)
        {
            return Path.Combine(reciter.LocalPath, GetFilePath(ayah, reciter));
        }

        public static string GetServerPathForAyah(QuranAyah ayah, ReciterItem reciter)
        {
            return Path.Combine(reciter.ServerUrl, GetFilePath(ayah, reciter));
        }

        private static string GetFilePath(QuranAyah ayah, ReciterItem reciter)
        {
            string fileName;
            if (reciter.IsGapless)
            {
                fileName = string.Format(reciter.GetFilePattern(), ayah.Surah);
            }
            else
            {
                if (ayah.Ayah == 0)
                {
                    fileName = string.Format(reciter.GetFilePattern(), 1, 1);
                }
                else
                {
                    fileName = string.Format(reciter.GetFilePattern(), ayah.Surah, ayah.Ayah);
                }
            }

            return fileName;
        }

        public static async Task<bool> DownloadRange(QuranAudioTrack request)
        {
            //var ayahToDownload = QuranUtils.GetAllAyah(request.FromAyah, request.ToAyah);
            //var filesToDownload = new List<string>();
            //bool result = true;

            //foreach (var ayah in ayahToDownload)
            //{
            //    filesToDownload.Add(GetServerPathForAyah(ayah, request.Reciter));
            //}

            //result = await QuranApp.DetailsViewModel.ActiveDownload.DownloadMultiple(filesToDownload.ToArray(),
            //            request.Reciter.LocalPath, Resources.loading_audio);

            //if (result)
            //{
            //    // attempt to download bismillah if it doesn't exist
            //    var bismillaFile = GetLocalPathForAyah(new QuranAyah(1, 1), request.Reciter);
            //    if (!await FileUtils.FileExists(bismillaFile))
            //    {
            //        QuranApp.NativeProvider.Log("bismillah doesn't exist, downloading...");
            //        result = await FileUtils.DownloadFileFromWebAsync(GetServerPathForAyah(new QuranAyah(1, 1), request.Reciter), 
            //            request.Reciter.LocalPath);
            //    }
            //}

            //return result;
            return true;
        }

        public static async Task<bool> DownloadGaplessRange(string urlString, string destination, QuranAyah fromAyah, QuranAyah toAyah)
        {
            var result = true;
            for (int i = fromAyah.Surah; i <= toAyah.Surah; i++)
            {
                string serverUrl = string.Format(CultureInfo.InvariantCulture, urlString, i);
                var localUrl = Path.Combine(destination, Path.GetFileName(urlString));
                QuranApp.NativeProvider.Log("gapless asking to download " + serverUrl + " to " + localUrl);

                result = await QuranApp.DetailsViewModel.ActiveDownload.DownloadSingleFile(serverUrl, localUrl, Resources.loading_audio);
                if (!result)
                    break;
            }
            return result;
        }

        //public static void PlayGapless(string localPath, QuranAyah ayah, ReciterItem reciter)
        //{
        //    var fileName = string.Format("{0:000}.mp3", ayah.Surah);
        //    var fullPath = Path.Combine(localPath, fileName);
        //    var fullPathAsUri = new Uri(fullPath, UriKind.Relative);
        //    var track = QuranApp.NativeProvider.AudioProvider.GetTrack();
        //    var title = QuranUtils.GetSurahAyahString(ayah.Surah, ayah.Ayah);
        //    if (track == null || track.Source != fullPathAsUri)
        //    {
        //        QuranApp.NativeProvider.AudioProvider.SetTrack(fullPathAsUri, title, reciter.Name, "Quran", null, null);
        //    }
        //    QuranApp.NativeProvider.AudioProvider.Play();
        //}

        //public static void PlayNonGapless(string localPath, QuranAyah ayah, ReciterItem reciter)
        //{
        //    var fileName = string.Format("{0:000}{1:000}.mp3", ayah.Surah, ayah.Ayah);
        //    var fullPath = Path.Combine(localPath, fileName);
        //    var fullPathAsUri = new Uri(fullPath, UriKind.Relative);
        //    var track = QuranApp.NativeProvider.AudioProvider.GetTrack();
        //    var title = QuranUtils.GetSurahAyahString(ayah.Surah, ayah.Ayah);
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
