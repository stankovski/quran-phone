using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Plugins.File;

namespace Quran.Core.Utils
{
    public static class FileUtils
    {
        public static bool failedToWrite = false;
        public static string IMG_HOST = "http://android.quran.com/data/";
        public static string QURAN_BASE = "quran_android";
        public static string QURAN_BASE_URI = "isostore:/" + QURAN_BASE;
        private static string AUDIO_DIRECTORY = "audio";
        private static string DATABASE_DIRECTORY = "databases";
        private static string DOWNLOADS_DIRECTORY = "downloads";
        private static string UNDELETED_FILES_DIRECTORY = "to-delete";
        public static string PACKAGE_NAME = "com.quran.labs.androidquran";
        public static string QURAN_ARABIC_DATABASE = "quran.ar.db";

        private static bool initialized = false;
        private static void Initialize()
        {
            if (initialized)
                return;

            initialized = true;

            // Initialize directory
            MakeQuranDirectory();
            MakeQuranDatabaseDirectory();

            // Delete stuck files
            DeleteStuckFiles();
        }

        private static IMvxFileStore FileStore
        {
            get
            {
                Initialize();
                return Mvx.Resolve<IMvxFileStore>();
            }
        }

        public static bool IsSeparator(this char c)
        {
            return c == '\\' || c == '/';
        }

        public static string Combine(params string[] paths)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var path in paths)
            {
                sb.Append(path);
                sb.Append("/");
            }
            return sb.ToString().Substring(0, sb.Length - 1);
        }

        public static string GetNativePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return string.Empty;

            return FileStore.NativePath(path);
        }

        /// <summary>
        /// Deletes folder even if it contains read only files
        /// </summary>
        /// <param name="path"></param>
        public static bool DeleteFolder(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            var files = FileStore.GetFilesIn(path);
            foreach (var file in files)
                DeleteFile(file);
            FileStore.DeleteFolder(path, false);
            return true;
        }

        public static bool DeleteFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            try
            {
                if (FileStore.Exists(path))
                    FileStore.DeleteFile(path);
                return true;
            }
            catch
            {
                var tempPath = GetUndeletedFilesDirectory(false, true);
                WriteFile(Combine(tempPath, string.Format("{0}.txt", Guid.NewGuid())), path);
                return false;
            }
        }

        public static void DeleteStuckFiles()
        {
            var path = GetUndeletedFilesDirectory(false, true);
            try
            {
                foreach (var fileName in FileStore.GetFilesIn(path))
                {
                    try
                    {
                        var filePath = Combine(path, fileName);
                        var badFilePath = ReadFile(filePath);
                        if (FileStore.Exists(badFilePath))
                            FileStore.DeleteFile(badFilePath);

                        FileStore.DeleteFile(filePath);
                    }
                    catch
                    {
                        // Continue
                    }
                }
            }
            catch
            {
                // Do nothing
            }
        }

        public static bool FileExists(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            return FileStore.Exists(path);
        }

        public static void MoveFile(string from, string to)
        {
            if (!string.IsNullOrWhiteSpace(from) &&
                !string.IsNullOrWhiteSpace(to))
            {
                FileStore.TryMove(from, to, true);
            }
        }

        public static bool DirectoryExists(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            return FileStore.FolderExists(path);
        }

        public static bool MakeDirectory(string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            FileStore.EnsureFolderExists(path);
            return true;
        }

        /// <summary>
        /// Creates directory recursively and writes no-media file in it
        /// </summary>
        /// <returns></returns>
        public static void MakeDirectoryRecursive(string path)
        {
            if (string.IsNullOrEmpty(path))
                return;

            if (!path[path.Length - 1].IsSeparator())
                path = string.Format("{0}/",path);

            for (int i = 0; i < path.Length; i++)
            {
                if (path[i].IsSeparator())
                {
                    var folder = path.Substring(0, i);
                    if (!string.IsNullOrEmpty(folder))
                        MakeDirectory(folder);
                }
            }
        }

        /// <summary>
        /// Creates Quran root directory and writes no-media file in it
        /// </summary>
        /// <returns></returns>
        public static bool MakeQuranDirectory()
        {
            string path = GetQuranDirectory(false);
            if (path == null)
                return false;

            if (MakeDirectory(path))
                return WriteNoMediaFile();
            else
                return false;
        }

        public static bool WriteFile(string path, string content)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            DeleteFile(path);

            try
            {
                FileStore.WriteFile(path, content);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool WriteFile(string path, Action<Stream> writeMethod)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            DeleteFile(path);

            try
            {
                FileStore.WriteFile(path, writeMethod);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static string ReadFile(string path)
        {
            if (FileExists(path))
            {
                string content = string.Empty;

                if (FileStore.TryReadTextFile(path, out content))
                    return content;
                else
                    return string.Empty;
            }
            else
            {
                return string.Empty;
            }
        }

        public static bool WriteNoMediaFile()
        {
            return WriteFile(GetQuranDirectory(false) + "/.nomedia", " ");
        }

        public static bool NoMediaFileExists()
        {
            return FileExists(GetQuranDirectory(false) + "/.nomedia");
        }

        public static void DeleteNoMediaFile()
        {
            DeleteFile(GetQuranDirectory(false) + "/.nomedia");
        }

        /// <summary>
        /// Creates Quran DB directory and writes no-media file in it
        /// </summary>
        /// <returns></returns>
        public static bool MakeQuranDatabaseDirectory()
        {
            string path = GetQuranDatabaseDirectory(false);
            if (path == null)
                return false;

            return MakeDirectory(path);
        }

        public static bool HaveAllImages()
        {
            if (FileStore.FolderExists(GetQuranDirectory(false)))
            {
                int count = FileStore.GetFilesIn(GetQuranDirectory(false)).Count();
                // Should have at least 95% of pages; of more than that it's not efficient to download the ZIP
                if (count >= 600)
                {
                    // ideally, we should loop for each page and ensure
                    // all pages are there, but this will do for now.
                    return true;
                }
            }
            else
            {
                MakeQuranDirectory();
            }

            return false;
        }

        /// <summary>
        /// Returns page name in the following format pageNNN.png
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static string GetPageFileName(int p)
        {
            return "page" + p.ToString("000", CultureInfo.InvariantCulture) + ".png";
        }

        public static Uri GetImageFromSD(string filename)
        {
            string location = GetQuranDirectory(true);
            if (location == null)
                return null;

            return new Uri(FileUtils.Combine(location, filename));
        }

        public static Uri GetImageFromWeb(string filename, bool useDriveIfExists = true)
        {
            MakeQuranDirectory();
            string path = FileUtils.Combine(GetQuranDirectory(false), filename);
            if (useDriveIfExists && FileUtils.FileExists(path))
            {
                return GetImageFromSD(filename);
            }
            else
            {
                ScreenUtils instance = ScreenUtils.Instance;
                if (instance == null) return null;

                string urlString = FileUtils.Combine(IMG_HOST + "width" + instance.GetWidthParam(),
                                                filename);
                return new Uri(urlString, UriKind.Absolute);                
            }
        }

        public static async Task<bool> DownloadFileFromWebAsync(string uri, string localPath)
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(uri);
                request.Method = HttpMethod.Get;
                var response = await request.GetResponseAsync();

                FileStore.WriteFile(localPath, stream => response.GetResponseStream().CopyTo(stream));
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static string GetQuranAudioDirectory(bool asUri, bool createIfDoesntExist = false)
        {
            return GetSubdirectory(asUri, AUDIO_DIRECTORY, createIfDoesntExist);
        }
        
        public static string GetQuranDatabaseDirectory(bool asUri, bool createIfDoesntExist = false)
        {
            return GetSubdirectory(asUri, DATABASE_DIRECTORY, createIfDoesntExist);
        }

        public static string GetDowloadTrackerDirectory(bool asUri, bool createIfDoesntExist = false)
        {
            return GetSubdirectory(asUri, DOWNLOADS_DIRECTORY, createIfDoesntExist);
        }

        public static string GetUndeletedFilesDirectory(bool asUri, bool createIfDoesntExist = false)
        {
            return GetSubdirectory(asUri, UNDELETED_FILES_DIRECTORY, createIfDoesntExist);
        }

        public static string GetSubdirectory(bool asUri, string name, bool createIfDoesntExist = false)
        {
            string baseDir = (asUri ? QURAN_BASE_URI : QURAN_BASE);

            if (createIfDoesntExist)
            {
                var tempPath = FileUtils.Combine(QURAN_BASE, name);
                MakeDirectoryRecursive(tempPath);
            }

            return (baseDir == null) ? null : FileUtils.Combine(baseDir, name);
        }

        public static string GetQuranDirectory(bool asUri, bool createIfDoesntExist = false)
        {
            string baseDir = (asUri ? QURAN_BASE_URI : QURAN_BASE);
            ScreenUtils qsi = ScreenUtils.Instance;
            if (qsi == null)
                return null;

            if (createIfDoesntExist)
            {
                var tempPath = QURAN_BASE + "width" + qsi.GetWidthParam();
                MakeDirectoryRecursive(tempPath);
            }

            return (baseDir == null) ? null : FileUtils.Combine(baseDir, "width" + qsi.GetWidthParam());
        }

        public static string GetZipFileUrl()
        {
            string url = IMG_HOST;
            ScreenUtils qsi = ScreenUtils.Instance;
            if (qsi == null)
                return null;
            url += "images" + qsi.GetWidthParam() + ".zip";
            return url;
        }

        public static bool ExtractZipFile(string source, string destination)
        {
            try
            {
                QuranApp.NativeProvider.ExtractZip(source, destination);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static string GetAyaPositionFileName()
        {
            ScreenUtils qsi = ScreenUtils.Instance;
            if (qsi == null) return null;
            return "ayahinfo" + qsi.GetWidthParam() + ".db";
        }

        public static string GetAyaPositionFileUrl()
        {
            ScreenUtils qsi = ScreenUtils.Instance;
            if (qsi == null)
                return null;
            string url = IMG_HOST + "width" + qsi.GetWidthParam();
            url += "/ayahinfo" + qsi.GetWidthParam() + ".zip";
            return url;
        }

        public static string GetGaplessDatabaseRootUrl()
        {
            ScreenUtils qsi = ScreenUtils.Instance;
            if (qsi == null)
                return null;
            return IMG_HOST + "databases/audio/";
        }

        public static bool HaveAyaPositionFile()
        {
            string baseDir = FileUtils.GetQuranDatabaseDirectory(false);
            if (baseDir == null)
                FileUtils.MakeQuranDatabaseDirectory();
            string filename = FileUtils.GetAyaPositionFileName();
            if (filename != null)
            {
                string ayaPositionDb = FileUtils.Combine(baseDir, filename);
                if (!FileUtils.FileExists(ayaPositionDb))
                {
                    return false;
                }
                else { return true; }
            }

            return false;
        }

        public static bool HaveArabicSearchFile()
        {
            string baseDir = FileUtils.GetQuranDatabaseDirectory(false);
            if (baseDir == null)
                FileUtils.MakeQuranDatabaseDirectory();
            string filename = FileUtils.QURAN_ARABIC_DATABASE;
            if (filename != null)
            {
                string arabicSearchDb = FileUtils.Combine(baseDir, filename);
                if (!FileUtils.FileExists(arabicSearchDb))
                {
                    return false;
                }
                else { return true; }
            }

            return false;
        }

        public static bool HasTranslation(string fileName)
        {
            string path = GetQuranDatabaseDirectory(false);
            if (path != null)
            {
                path = FileUtils.Combine(path, fileName);
                return FileExists(path);
            }
            return false;
        }

        public static bool RemoveTranslation(string fileName)
        {
            string path = GetQuranDatabaseDirectory(false);
            if (path != null)
            {
                path = FileUtils.Combine(path, fileName);

                if (FileStore.Exists(path))
                    FileStore.DeleteFile(path);

                return true;
            }
            return false;
        }

        //public static bool hasArabicSearchDatabase(){
        //   return hasTranslation(QuranDataProvider.QURAN_ARABIC_DATABASE);
        //}

        public static string GetArabicSearchUrl()
        {
            return IMG_HOST + DATABASE_DIRECTORY + "/" + QURAN_ARABIC_DATABASE;
        }

        public static bool IsFileEmpty(string localFilePath)
        {
            try
            {
                string content;
                if (FileStore.TryReadTextFile(localFilePath, out content))
                    return string.IsNullOrWhiteSpace(content);
                else
                    return true;
            }
            catch
            {
                return true;
            }
        }
    }
}
