using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.IsolatedStorage;
using System.Globalization;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using System.Net;
using System.Threading;
using ICSharpCode.SharpZipLib.Zip;
using Salient.SharpZipLib.Zip;
using QuranPhone.Data;

namespace QuranPhone.Utils
{
    public static class QuranFileUtils
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

        public static bool IsSeparator(this char c)
        {
            return c == '\\' || c == '/';
        }

        /// <summary>
        /// Deletes folder even if it contains read only files
        /// </summary>
        /// <param name="path"></param>
        public static void DeleteFolder(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentNullException("path");

            using (var isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!isf.DirectoryExists(path))
                    return;

                foreach (var dir in isf.GetDirectoryNames(path + "/*"))
                {
                    DeleteFolder(path + "/" + dir);
                }
                foreach (var file in isf.GetFileNames(path + "/*.*"))
                {
                    isf.DeleteFile(path + "/" + file);
                }
                isf.DeleteDirectory(path);
            }
        }

        public static void DeleteFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentNullException("path");

            using (var isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                try
                {
                    if (isf.FileExists(path))
                        isf.DeleteFile(path);
                }
                catch
                {
                    var tempPath = GetUndeletedFilesDirectory(false, true);
                    WriteFile(Path.Combine(tempPath, string.Format("{0}.txt", Guid.NewGuid())), path);
                }
            }
        }

        public static void DeleteStuckFiles()
        {
            using (var isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                var path = GetUndeletedFilesDirectory(false, true);
                try
                {
                    foreach (var fileName in isf.GetFileNames(string.Format("{0}/*", path)))
                    {
                        try
                        {
                            var filePath = Path.Combine(path, fileName);
                            var badFilePath = ReadFile(filePath);
                            if (isf.FileExists(badFilePath))
                                isf.DeleteFile(badFilePath);

                            isf.DeleteFile(filePath);
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
        }

        public static bool FileExists(string path) 
        {
            using (var isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                return isf.FileExists(path);
            }
        }

        public static void MoveFile(string from, string to)
        {
            using (var isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (isf.FileExists(to))
                    isf.DeleteFile(to);
                if (isf.FileExists(from))
                {
                    isf.MoveFile(from, to);
                }
            }
        }

        public static bool DirectoryExists(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentNullException("path");

            using (var isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                return isf.DirectoryExists(path);
            }
        }

        /// <summary>
        /// Creates directory and writes no-media file in it
        /// </summary>
        /// <returns></returns>
        public static bool MakeDirectory(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentNullException("path");

            using (var isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (isf.DirectoryExists(path))
                {
                    return true;
                }
                else
                {
                    try
                    {
                        isf.CreateDirectory(path);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
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
            DeleteFile(path);

            using (var isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                try
                {
                    using (var isoStream = new IsolatedStorageFileStream(path, FileMode.Create, isf))
                    {
                        using (var writer = new StreamWriter(isoStream))
                        {
                            writer.Write(content);
                        }
                    }
                    return true;
                }
                catch (IOException)
                {
                    return false;
                }
            }
        }

        public static string ReadFile(string path)
        {
            if (FileExists(path))
            {
                using (var isf = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    try
                    {
                        using (var isoStream = new IsolatedStorageFileStream(path, FileMode.Open, isf))
                        {
                            using (var reader = new StreamReader(isoStream))
                            {
                                return reader.ReadToEnd();
                            }
                        }
                    }
                    catch (IOException е)
                    {
                        return string.Empty;
                    }
                }
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
            using (var isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (isf.DirectoryExists(GetQuranDirectory(false)))
                {
                    var files = isf.GetFileNames(Path.Combine(GetQuranDirectory(false), "*.png"));
                    // Should have at least 95% of pages; of more than that it's not efficient to download the ZIP
                    if (files.Length >= 600)
                    {
                        // ideally, we should loop for each page and ensure
                        // all pages are there, but this will do for now.
                        return true;
                    }
                }
                else
                {
                    QuranFileUtils.MakeQuranDirectory();
                }
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

            return new Uri(Path.Combine(location, filename));
        }

        public static bool IncreaseIsolatedStorageSpace(long quotaSizeDemand)
        {
            bool canSizeIncrease = false;
            using (var isolatedStorageFile = IsolatedStorageFile.GetUserStoreForApplication())
            {

                //Get the Available space
                long maxAvailableSpace = isolatedStorageFile.AvailableFreeSpace;

                if (quotaSizeDemand > maxAvailableSpace)
                {
                    if (!isolatedStorageFile.IncreaseQuotaTo(isolatedStorageFile.Quota + quotaSizeDemand))
                    {
                        canSizeIncrease = false;
                        return canSizeIncrease;
                    }

                    canSizeIncrease = true;
                    return canSizeIncrease;
                }
                return canSizeIncrease;
            }
        }

        public static Uri GetImageFromWeb(string filename, bool useDriveIfExists = true)
        {
            MakeQuranDirectory();
            string path = Path.Combine(GetQuranDirectory(false), filename);
            if (useDriveIfExists && QuranFileUtils.FileExists(path))
            {
                return GetImageFromSD(filename);
            }
            else
            {
                QuranScreenInfo instance = QuranScreenInfo.Instance;
                if (instance == null) return null;

                string urlString = Path.Combine(IMG_HOST + "width" + instance.GetWidthParam(),
                                                filename);
                return new Uri(urlString, UriKind.Absolute);                
            }
        }

        public static async Task<bool> DownloadFileFromWebAsync(string uri, string localPath)
        {
            using (var isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                try
                {
                    var request = (HttpWebRequest)WebRequest.Create(uri);
                    request.Method = HttpMethod.Get;
                    var response = await request.GetResponseAsync();
                    using (var isfStream = new IsolatedStorageFileStream(localPath, FileMode.Create, isf))
                    using (var sr = new StreamReader(response.GetResponseStream()))
                    {
                        response.GetResponseStream().CopyTo(isfStream);
                    }
                    return true;
                }
                catch
                {
                    return false;
                }
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
                var tempPath = Path.Combine(QURAN_BASE, name);
                MakeDirectoryRecursive(tempPath);
            }

            return (baseDir == null) ? null : Path.Combine(baseDir, name);
        }

        public static string GetQuranDirectory(bool asUri, bool createIfDoesntExist = false)
        {
            string baseDir = (asUri ? QURAN_BASE_URI : QURAN_BASE);
            QuranScreenInfo qsi = QuranScreenInfo.Instance;
            if (qsi == null)
                return null;

            if (createIfDoesntExist)
            {
                var tempPath = QURAN_BASE + "width" + qsi.GetWidthParam();
                MakeDirectoryRecursive(tempPath);
            }

            return (baseDir == null) ? null : Path.Combine(baseDir, "width" + qsi.GetWidthParam());
        }

        public static string GetZipFileUrl()
        {
            string url = IMG_HOST;
            QuranScreenInfo qsi = QuranScreenInfo.Instance;
            if (qsi == null)
                return null;
            url += "images" + qsi.GetWidthParam() + ".zip";
            return url;
        }

        public static bool ExtractZipFile(string source, string destination)
        {
            try
            {
                new IsolatedFastZip().ExtractZip(source, destination, "");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("error unziping data:" + ex.Message);
                return false;
            }
        }

        public static string GetAyaPositionFileName()
        {
            QuranScreenInfo qsi = QuranScreenInfo.Instance;
            if (qsi == null) return null;
            return "ayahinfo" + qsi.GetWidthParam() + ".db";
        }

        public static string GetAyaPositionFileUrl()
        {
            QuranScreenInfo qsi = QuranScreenInfo.Instance;
            if (qsi == null)
                return null;
            string url = IMG_HOST + "width" + qsi.GetWidthParam();
            url += "/ayahinfo" + qsi.GetWidthParam() + ".zip";
            return url;
        }

        public static string GetGaplessDatabaseRootUrl()
        {
            QuranScreenInfo qsi = QuranScreenInfo.Instance;
            if (qsi == null)
                return null;
            return IMG_HOST + "databases/audio/";
        }

        public static bool HaveAyaPositionFile()
        {
            string baseDir = QuranFileUtils.GetQuranDatabaseDirectory(false);
            if (baseDir == null)
                QuranFileUtils.MakeQuranDatabaseDirectory();
            string filename = QuranFileUtils.GetAyaPositionFileName();
            if (filename != null)
            {
                string ayaPositionDb = Path.Combine(baseDir, filename);
                if (!QuranFileUtils.FileExists(ayaPositionDb))
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
                path = Path.Combine(path, fileName);
                return FileExists(path);
            }
            return false;
        }

        public static bool RemoveTranslation(string fileName)
        {
            string path = GetQuranDatabaseDirectory(false);
            if (path != null)
            {
                path = Path.Combine(path, fileName);
                using (var isf = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (isf.FileExists(path))
                        isf.DeleteFile(path);
                }
                return true;
            }
            return false;
        }

        //public static bool hasArabicSearchDatabase(){
        //   return hasTranslation(QuranDataProvider.QURAN_ARABIC_DATABASE);
        //}

        public static string GetArabicSearchDatabaseUrl()
        {
            return IMG_HOST + DATABASE_DIRECTORY + "/" + QURAN_ARABIC_DATABASE;
        }        
    }
}
