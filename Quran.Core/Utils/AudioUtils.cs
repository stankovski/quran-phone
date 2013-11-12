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
    public enum LookAheadAmount
    {
        PAGE = 1,
        SURA = 2,
        JUZ = 3
    }

    public class AudioUtils
    {
        public const string DB_EXTENSION = ".db";
        public const string AUDIO_EXTENSION = ".mp3";
        public const string ZIP_EXTENSION = ".zip";
        public const int MIN = 1;
        public const int MAX = 3;
        private static string[] mQariBaseUrls = null;
        private static string[] mQariFilePaths = null;
        private static string[] mQariDatabaseFiles = null;
        private static string[] mQariNames = null;
        private static ReciterItem[] mReciterItems = null;

        #region Public Methods

        public static string[] ReciterNames
        {
            get
            {
                if (mQariNames == null)
                    mQariNames = GetResources("quran_readers_name");
                return mQariNames;
            }
        }


        public static int GetReciterPositionByName(string name)
        {
            if (name == null)
                return -1;

            for (int i = 0; i < ReciterNames.Length; i++)
            {
                if (ReciterNames[i].Equals(name, System.StringComparison.OrdinalIgnoreCase))
                    return i;
            }
            return 0;
        }

        public static ReciterItem[] GetReciterItems()
        {
            if (mReciterItems != null)
                return mReciterItems;

            var reciterItems = new List<ReciterItem>();
            for (int i = 0; i < ReciterNames.Length; i++)
            {
                reciterItems.Add(new ReciterItem
                {
                    Id = i,
                    Name = ReciterNames[i],
                    ServerUrl = GetReciterUrl(i, true),
                    LocalPath = GetLocalReciterUrl(i),
                    IsGapless = IsQariGapless(i),
                    GaplessDatabasePath = GetQariDatabasePathIfGapless(i)
                });
            }
            mReciterItems = reciterItems.ToArray();

            return mReciterItems;
        }

        public static ReciterItem GetReciterItem(int position)
        {
            return GetReciterItems()[position];
        }

        public static string GetQariDatabasePathIfGapless(int position)
        {
            if (position > ReciterDatabaseFiles.Length)
            {
                return null;
            }

            string dbname = ReciterDatabaseFiles[position];
            if (string.IsNullOrEmpty(dbname))
            {
                return null;
            }

            string path = GetLocalReciterUrl(position);
            if (path == null)
            {
                return null;
            }
            return FileUtils.Combine(path, dbname + DB_EXTENSION);
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

        public static QuranAyah GetLastAyahToPlay(QuranAyah startAyah, int page, LookAheadAmount mode)
        {
            int pageLastSura = 114;
            int pageLastAyah = 6;
            if (page > 604 || page < 0)
            {
                return null;
            }
            if (page < 604)
            {
                int nextPageSura = QuranInfo.PAGE_SURA_START[page];
                int nextPageAyah = QuranInfo.PAGE_AYAH_START[page];

                pageLastSura = nextPageSura;
                pageLastAyah = nextPageAyah - 1;
                if (pageLastAyah < 1)
                {
                    pageLastSura--;
                    if (pageLastSura < 1)
                    {
                        pageLastSura = 1;
                    }
                    pageLastAyah = QuranInfo.GetSuraNumberOfAyah(pageLastSura);
                }
            }

            if (mode == LookAheadAmount.SURA)
            {
                int sura = startAyah.Sura;
                int lastAyah = QuranInfo.GetSuraNumberOfAyah(sura);
                if (lastAyah == -1)
                {
                    return null;
                }

                // if we start playback between two suras, download both suras
                if (pageLastSura > sura)
                {
                    sura = pageLastSura;
                    lastAyah = QuranInfo.GetSuraNumberOfAyah(sura);
                }
                return new QuranAyah(sura, lastAyah);
            }
            else if (mode == LookAheadAmount.JUZ)
            {
                int juz = QuranInfo.GetJuzFromPage(page);
                if (juz == 30)
                {
                    return new QuranAyah(114, 6);
                }
                else if (juz >= 1 && juz < 30)
                {
                    int[] endJuz = QuranInfo.QUARTERS[juz * 8];
                    if (pageLastSura > endJuz[0])
                    {
                        // ex between jathiya and a7qaf
                        endJuz = QuranInfo.QUARTERS[(juz + 1) * 8];
                    }
                    else if (pageLastSura == endJuz[0] &&
                             pageLastAyah > endJuz[1])
                    {
                        // ex surat al anfal
                        endJuz = QuranInfo.QUARTERS[(juz + 1) * 8];
                    }

                    return new QuranAyah(endJuz[0], endJuz[1]);
                }
            }

            // page mode (fallback also from errors above)
            return new QuranAyah(pageLastSura, pageLastAyah);
        }

        public static bool ShouldDownloadBasmallah(AudioRequest request)
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
                    string filename = string.Format("1\\1{0}", AUDIO_EXTENSION);
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

            return DoesRequireBasmallah(request);
        }

        public static bool HaveSuraAyahForQari(string baseDir, int sura, int ayah)
        {
            string filename = FileUtils.Combine(baseDir, sura + "\\" + ayah + AUDIO_EXTENSION);
            return FileUtils.FileExists(filename);
        }

        public static bool DoesRequireBasmallah(AudioRequest request)
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
                    if (j == 1 && i != 1 && i != 9)
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
            {
                return false;
            }

            bool isGapless = request.Reciter.IsGapless;
            if (!FileUtils.DirectoryExists(baseDirectory))
            {
                FileUtils.MakeDirectory(baseDirectory);
                return false;
            }

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

                if (isGapless)
                {
                    if (i == endSura && endAyah == 0)
                    {
                        continue;
                    }
                    string p = request.Reciter.ServerUrl;
                    string fileName = string.Format(p, i);
                    if (!FileUtils.FileExists(fileName))
                    {
                        return false;
                    }
                    continue;
                }

                for (int j = firstAyah; j < lastAyah; j++)
                {
                    string filename = string.Format(GetFilePattern(request.Reciter.Id), i, j);
                    if (!FileUtils.FileExists(FileUtils.Combine(baseDirectory, filename)))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public static async Task<bool> DownloadRange(string urlString, string destination, QuranAyah fromAyah, QuranAyah toAyah)
        {
            FileUtils.MakeDirectory(destination);

            int totalAyahs = 0;

            if (fromAyah.Sura == toAyah.Sura)
            {
                totalAyahs = toAyah.Ayah - fromAyah.Ayah + 1;
            }
            else
            {
                // add the number ayahs from suras in between start and end
                for (int i = fromAyah.Sura + 1; i < toAyah.Sura; i++)
                {
                    totalAyahs += QuranInfo.GetSuraNumberOfAyah(i);
                }

                // add the number of ayahs from the start sura
                totalAyahs += QuranInfo.GetSuraNumberOfAyah(fromAyah.Sura) - fromAyah.Ayah + 1;

                // add the number of ayahs from the last sura
                totalAyahs += toAyah.Ayah;
            }

            // extension and filename template don't change
            string filename = PathHelper.GetFileName(urlString);
            int extLocation = filename.LastIndexOf('.');
            string extension = filename.Substring(extLocation);

            bool result = true;
            for (int i = fromAyah.Sura; i <= toAyah.Sura; i++)
            {
                int lastAyah = QuranInfo.GetSuraNumberOfAyah(i);
                if (i == toAyah.Sura) { lastAyah = toAyah.Ayah; }
                int firstAyah = 1;
                if (i == fromAyah.Sura) { firstAyah = fromAyah.Ayah; }

                // same destination directory for ayahs within the same sura
                FileUtils.MakeDirectory(destination);
                var filesToDownload = new List<string>();

                for (int j = firstAyah; j <= lastAyah; j++)
                {
                    string url = string.Format(CultureInfo.InvariantCulture, urlString, i, j);

                    filesToDownload.Add(url);
                }
                result = await QuranApp.DetailsViewModel.ActiveDownload.DownloadMultiple(filesToDownload.ToArray(), destination,
                                AppResources.loading_audio);
            }

            if (result)
            {
                // attempt to download basmallah if it doesn't exist
                string destDir = destination + "/" + 1 + "/";
                FileUtils.MakeDirectory(destDir);

                if (!FileUtils.FileExists(PathHelper.Combine(destDir, "1" + extension)))
                {
                    QuranApp.NativeProvider.Log("basmallah doesn't exist, downloading...");
                    string url = string.Format(CultureInfo.InvariantCulture, urlString, 1, 1);
                    string destFile = 1 + extension;

                    result = await FileUtils.DownloadFileFromWebAsync(url, PathHelper.Combine(destDir, destFile));

                    if (!result) { return false; }
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
        #endregion

        #region Private Methods
        private static string[] ReciterBaseUrls
        {
            get
            {
                if (mQariBaseUrls == null)
                    mQariBaseUrls = GetResources("quran_readers_urls");
                return mQariBaseUrls;
            }
        }

        private static string[] ReciterDatabaseFiles
        {
            get
            {
                if (mQariDatabaseFiles == null)
                    mQariDatabaseFiles = GetResources("quran_readers_db_name");
                return mQariDatabaseFiles;
            }
        }

        private static string[] ReciterFilePaths
        {
            get
            {
                if (mQariFilePaths == null)
                    mQariFilePaths = GetResources("quran_readers_path");
                return mQariFilePaths;
            }
        }

        private static string[] GetResources(string name)
        {
            var resourceLoader = Mvx.Resolve<IMvxResourceLoader>();
            string[] returnValue = null;
            resourceLoader.GetResourceStream("Assets/quran_readers_urls.xml", (s) =>
                {
                    var xmlDoc = XDocument.Load(s);
                    returnValue = xmlDoc.Descendants(name).First().Descendants().Select(e => e.Value).ToArray();
                });
            return returnValue;
        }

        private static string GetReciterUrl(int position, bool addPlaceHolders)
        {
            if (position >= ReciterBaseUrls.Length || 0 > position)
            {
                return null;
            }
            string url = ReciterBaseUrls[position];
            if (addPlaceHolders)
            {
                url += GetFilePattern(position);
            }
            return url;
        }

        private static string GetFilePattern(int position)
        {
            if (position >= ReciterBaseUrls.Length || 0 > position)
            {
                return null;
            }

            if (IsQariGapless(position))
            {
                return "{0:000}" + AUDIO_EXTENSION;
            }
            else
            {
                return "{0:000}{1:000}" + AUDIO_EXTENSION;
            }
        }

        private static string GetLocalReciterUrl(int position)
        {
            string rootDirectory = FileUtils.GetQuranAudioDirectory(false);
            return rootDirectory == null
                       ? null
                       : FileUtils.Combine(rootDirectory, ReciterFilePaths[position]);
        }

        private static bool IsQariGapless(int position)
        {
            return GetQariDatabasePathIfGapless(position) != null;
        }


        #endregion
    }
}
