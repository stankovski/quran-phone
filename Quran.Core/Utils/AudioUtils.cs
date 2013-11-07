using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Cirrious.CrossCore;
using Cirrious.CrossCore.Platform;
using Quran.Core.Data;
using Quran.Core.Common;
using Quran.Core.Data;

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
        private const string AUDIO_DIRECTORY = "audio";
        public const int MIN = 1;
        public const int MAX = 3;
        private static string[] mQariBaseUrls = null;
        private static string[] mQariFilePaths = null;
        private static string[] mQariDatabaseFiles = null;
        private static string[] mQariNames = null;
        private static ReciterItem[] mReciterItems = null;

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

        public static int GetQariPositionByName(string name)
        {
            if (mQariNames == null || name == null)
                return 0;

            for (int i = 0; i < mQariNames.Length; i++)
            {
                if (mQariNames[i].Equals(name, System.StringComparison.OrdinalIgnoreCase))
                    return i;
            }
            return 0;
        }

        public static string GetQariNameByPosition(int position)
        {
            if (mQariNames == null || position >= mQariNames.Length || position < 0)
                return null;

            return mQariNames[position];
        }

        public static string GetQariUrl(int position, bool addPlaceHolders)
        {
            if (mQariBaseUrls == null)
            {
                mQariBaseUrls = GetResources("quran_readers_urls");
            }

            if (position >= mQariBaseUrls.Length || 0 > position)
            {
                return null;
            }
            string url = mQariBaseUrls[position];
            if (addPlaceHolders)
            {
                if (IsQariGapless(position))
                {
                    url += "{0:000}" + AUDIO_EXTENSION;
                }
                else
                {
                    url += "{0:000}{1:000}" + AUDIO_EXTENSION;
                }
            }
            return url;
        }

        public static string GetLocalQariUrl(int position)
        {
            if (mQariFilePaths == null)
            {
                mQariFilePaths = GetResources("quran_readers_path");
            }

            string rootDirectory = QuranFileUtils.GetQuranAudioDirectory(false);
            return rootDirectory == null
                       ? null
                       : QuranFileUtils.Combine(rootDirectory, mQariFilePaths[position]);
        }

        public static bool IsQariGapless(int position)
        {
            return GetQariDatabasePathIfGapless(position) != null;
        }

        public static ReciterItem[] GetReciterItems()
        {
            if (mReciterItems != null)
                return mReciterItems;
            
            if (mQariNames == null)
            {
                mQariNames = GetResources("quran_readers_name");
            }

            mReciterItems = new ReciterItem[mQariNames.Length];
            for (int i = 0; i < mQariNames.Length; i++)
            {
                mReciterItems[i] = new ReciterItem
                {
                    Id = i,
                    Name = mQariNames[i],
                    ServerUrl = GetQariUrl(i, true),
                    LocalPath = GetLocalQariUrl(i),
                    IsGapless = IsQariGapless(i),
                    DatabaseName = GetQariDatabasePathIfGapless(i)
                };
            }

            return mReciterItems;
        }

        public static string GetQariDatabasePathIfGapless(int position)
        {
            if (mQariDatabaseFiles == null)
            {
                mQariDatabaseFiles = GetResources("quran_readers_db_name");
            }

            if (position > mQariDatabaseFiles.Length)
            {
                return null;
            }

            string dbname = mQariDatabaseFiles[position];
            if (string.IsNullOrEmpty(dbname))
            {
                return null;
            }

            string path = GetLocalQariUrl(position);
            if (path == null)
            {
                return null;
            }
            return QuranFileUtils.Combine(path, dbname + DB_EXTENSION);
        }

        public static bool ShouldDownloadGaplessDatabase(AudioRequest request)
        {
            if (!request.IsGapless())
            {
                return false;
            }
            string dbPath = request.GetGaplessDatabaseFilePath();
            if (string.IsNullOrEmpty(dbPath))
            {
                return false;
            }

            return !QuranFileUtils.FileExists(dbPath);
        }

        public static string GetGaplessDatabaseUrl(AudioRequest request)
        {
            if (!request.IsGapless())
            {
                return null;
            }
            int qariId = request.GetQariId();

            if (mQariDatabaseFiles == null)
            {
                mQariDatabaseFiles = GetResources("quran_readers_db_name");
            }

            if (qariId > mQariDatabaseFiles.Length)
            {
                return null;
            }

            string dbname = mQariDatabaseFiles[qariId] + ZIP_EXTENSION;
            return PathHelper.Combine(QuranFileUtils.GetGaplessDatabaseRootUrl(), dbname);
        }

        public static QuranAyah GetLastAyahToPlay(QuranAyah startAyah,
                                                  int page, LookAheadAmount mode)
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
                    pageLastAyah = QuranInfo.GetNumAyahs(pageLastSura);
                }
            }

            if (mode == LookAheadAmount.SURA)
            {
                int sura = startAyah.Sura;
                int lastAyah = QuranInfo.GetNumAyahs(sura);
                if (lastAyah == -1)
                {
                    return null;
                }

                // if we start playback between two suras, download both suras
                if (pageLastSura > sura)
                {
                    sura = pageLastSura;
                    lastAyah = QuranInfo.GetNumAyahs(sura);
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
                    int[] endJuz = QuranInfo.QUARTERS[juz*8];
                    if (pageLastSura > endJuz[0])
                    {
                        // ex between jathiya and a7qaf
                        endJuz = QuranInfo.QUARTERS[(juz + 1)*8];
                    }
                    else if (pageLastSura == endJuz[0] &&
                             pageLastAyah > endJuz[1])
                    {
                        // ex surat al anfal
                        endJuz = QuranInfo.QUARTERS[(juz + 1)*8];
                    }

                    return new QuranAyah(endJuz[0], endJuz[1]);
                }
            }

            // page mode (fallback also from errors above)
            return new QuranAyah(pageLastSura, pageLastAyah);
        }

        public static bool ShouldDownloadBasmallah(AudioRequest request)
        {
            if (request.IsGapless())
            {
                return false;
            }
            string baseDirectory = request.GetLocalPath();
            if (!string.IsNullOrEmpty(baseDirectory))
            {
                if (QuranFileUtils.DirectoryExists(baseDirectory))
                {
                    string filename = string.Format("1\\1{0}", AUDIO_EXTENSION);
                    if (QuranFileUtils.FileExists(QuranFileUtils.Combine(baseDirectory, filename)))
                    {
                        return false;
                    }
                }
                else
                {
                    QuranFileUtils.MakeDirectory(baseDirectory);
                }
            }

            return DoesRequireBasmallah(request);
        }

        public static bool HaveSuraAyahForQari(string baseDir, int sura, int ayah)
        {
            string filename = QuranFileUtils.Combine(baseDir, sura + "\\" + ayah + AUDIO_EXTENSION);
            return QuranFileUtils.FileExists(filename);
        }

        private static bool DoesRequireBasmallah(AudioRequest request)
        {
            QuranAyah minAyah = request.GetMinAyah();
            int startSura = minAyah.Sura;
            int startAyah = minAyah.Ayah;

            QuranAyah maxAyah = request.GetMaxAyah();
            int endSura = maxAyah.Sura;
            int endAyah = maxAyah.Ayah;

            for (int i = startSura; i <= endSura; i++)
            {
                int lastAyah = QuranInfo.GetNumAyahs(i);
                if (i == endSura)
                {
                    lastAyah = endAyah;
                }
                int firstAyah = 1;
                if (i == startSura)
                {
                    firstAyah = startAyah;
                }

                for (int j = firstAyah; j < lastAyah; j++)
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
            string baseDirectory = request.GetLocalPath();
            if (string.IsNullOrEmpty(baseDirectory))
            {
                return false;
            }

            bool isGapless = request.IsGapless();
            if (!QuranFileUtils.DirectoryExists(baseDirectory))
            {
                QuranFileUtils.MakeDirectory(baseDirectory);
                return false;
            }

            QuranAyah minAyah = request.GetMinAyah();
            int startSura = minAyah.Sura;
            int startAyah = minAyah.Ayah;

            QuranAyah maxAyah = request.GetMaxAyah();
            int endSura = maxAyah.Sura;
            int endAyah = maxAyah.Ayah;

            for (int i = startSura; i <= endSura; i++)
            {
                int lastAyah = QuranInfo.GetNumAyahs(i);
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
                    string p = request.GetBaseUrl();
                    string fileName = string.Format(p, i);
                    if (!QuranFileUtils.FileExists(fileName))
                    {
                        return false;
                    }
                    continue;
                }

                for (int j = firstAyah; j < lastAyah; j++)
                {
                    string filename = QuranFileUtils.Combine(i.ToString(CultureInfo.InvariantCulture), j + AUDIO_EXTENSION);
                    if (!QuranFileUtils.FileExists(QuranFileUtils.Combine(baseDirectory, filename)))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
