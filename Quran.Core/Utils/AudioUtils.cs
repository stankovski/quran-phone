using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Quran.Core.Data;
using Quran.Core.Common;
using Quran.Core.Properties;
using System.IO;
using Windows.Storage;
using Windows.Storage.Search;

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
        public const string BismillahFile = "001001.mp3";
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

        public static async Task<bool> ShouldDownloadBismillah(StorageFolder baseDirectory, QuranAudioTrack request)
        {
            if (request.GetReciter().IsGapless || !DoesRequireBismillah(request))
            {
                return false;
            }
            
            return await FileUtils.FileExists(baseDirectory, BismillahFile);
        }

        public static bool DoesRequireBismillah(QuranAudioTrack request)
        {
            return QuranUtils.HasBismillah(request.Surah);
        }

        public static async Task<List<string>> GetMissingFiles(QuranAudioTrack request)
        {
            List<string> missingFiles = new List<string>();
            var reciter = request.GetReciter();
            var baseDirectory = await request.GetReciter().GetStorageFolder();
            
            if (await ShouldDownloadBismillah(baseDirectory, request))
            {
                missingFiles.Add(BismillahFile);
            }

            var fileQuery = baseDirectory.CreateFileQueryWithOptions(new QueryOptions
            {
                UserSearchFilter = string.Format(CultureInfo.InvariantCulture,
                    "{0:000}*.mp3", request.Surah),
                FolderDepth = FolderDepth.Shallow
            });

            if ((uint)QuranUtils.GetSurahNumberOfAyah(request.Surah) != await fileQuery.GetItemCountAsync())
            {
                var existingFiles = new HashSet<string>((await fileQuery.GetFilesAsync()).Select(f => f.Name));
                for (int i = 1; i <= QuranUtils.GetSurahNumberOfAyah(request.Surah); i++)
                {
                    var filePath = GetFilePath(new QuranAyah(request.Surah, i), reciter);
                    if (!existingFiles.Contains(filePath))
                    {
                        missingFiles.Add(filePath);
                    }
                }
            }

            return missingFiles;
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
