using System;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using QuranPhone.SharpZipLib.IsolatedStorage.Zip;

namespace QuranPhone.Utils
{
    public static class QuranFileUtils
    {
        public const string ImgHost = "http://android.quran.com/data/";
        public const string QuranBase = "quran_android";
        public const string QuranBaseUri = "isostore:/" + QuranBase;
        public const string DatabaseDirectory = "databases";
        public const string DownloadsDirectory = "downloads";
        public const string UndeletedFilesDirectory = "to-delete";
        public const string QuranArabicDatabase = "quran.ar.db";

        public static bool IsSeparator(this char c)
        {
            return c == '\\' || c == '/';
        }

        public static void DeleteFolder(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException("Path is empty");
            }

            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!isf.DirectoryExists(path))
                {
                    return;
                }

                foreach (string dir in isf.GetDirectoryNames(path + "/*"))
                {
                    DeleteFolder(path + "/" + dir);
                }
                foreach (string file in isf.GetFileNames(path + "/*.*"))
                {
                    isf.DeleteFile(path + "/" + file);
                }
                isf.DeleteDirectory(path);
            }
        }

        public static void DeleteFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException("path");
            }

            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                try
                {
                    if (isf.FileExists(path))
                    {
                        isf.DeleteFile(path);
                    }
                }
                catch
                {
                    string tempPath = GetUndeletedFilesDirectory(false, true);
                    WriteFile(Path.Combine(tempPath, string.Format("{0}.txt", Guid.NewGuid())), path);
                }
            }
        }

        public static void DeleteStuckFiles()
        {
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                string path = GetUndeletedFilesDirectory(false, true);
                try
                {
                    foreach (string fileName in isf.GetFileNames(string.Format("{0}/*", path)))
                    {
                        try
                        {
                            string filePath = Path.Combine(path, fileName);
                            string badFilePath = ReadFile(filePath);
                            if (isf.FileExists(badFilePath))
                            {
                                isf.DeleteFile(badFilePath);
                            }

                            isf.DeleteFile(filePath);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message);
                        }
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
            }
        }

        public static bool FileExists(string path)
        {
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                return isf.FileExists(path);
            }
        }

        public static void MoveFile(string from, string to)
        {
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (isf.FileExists(to))
                {
                    isf.DeleteFile(to);
                }
                if (isf.FileExists(from))
                {
                    isf.MoveFile(from, to);
                }
            }
        }

        /// <summary>
        ///     Creates directory and writes no-media file in it
        /// </summary>
        /// <returns></returns>
        public static bool MakeDirectory(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException("path");
            }

            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (isf.DirectoryExists(path))
                {
                    return true;
                }
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

        /// <summary>
        ///     Creates directory recursively and writes no-media file in it
        /// </summary>
        /// <returns></returns>
        public static void MakeDirectoryRecursive(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            if (!path[path.Length - 1].IsSeparator())
            {
                path = string.Format("{0}/", path);
            }

            for (int i = 0; i < path.Length; i++)
            {
                if (path[i].IsSeparator())
                {
                    string folder = path.Substring(0, i);
                    if (!string.IsNullOrEmpty(folder))
                    {
                        MakeDirectory(folder);
                    }
                }
            }
        }

        /// <summary>
        ///     Creates Quran root directory and writes no-media file in it
        /// </summary>
        /// <returns></returns>
        public static void MakeQuranDirectory()
        {
            string path = GetQuranDirectory(false);
            if (path == null)
            {
                return;
            }

            if (MakeDirectory(path))
            {
                WriteNoMediaFile();
            }
        }

        public static void WriteFile(string path, string content)
        {
            DeleteFile(path);

            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
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
                }
                catch (IOException)
                {}
            }
        }

        public static string ReadFile(string path)
        {
            if (FileExists(path))
            {
                using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
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
                    catch
                    {
                        return string.Empty;
                    }
                }
            }
            return string.Empty;
        }

        public static void WriteNoMediaFile()
        {
            WriteFile(GetQuranDirectory(false) + "/.nomedia", " ");
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
        ///     Creates Quran DB directory and writes no-media file in it
        /// </summary>
        /// <returns></returns>
        public static void MakeQuranDatabaseDirectory()
        {
            string path = GetQuranDatabaseDirectory(false);
            if (path == null)
            {
                return;
            }

            MakeDirectory(path);
        }

        public static bool HaveAllImages()
        {
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (isf.DirectoryExists(GetQuranDirectory(false)))
                {
                    string[] files = isf.GetFileNames(Path.Combine(GetQuranDirectory(false), "*.png"));
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
                    MakeQuranDirectory();
                }
            }

            return false;
        }

        /// <summary>
        ///     Returns page name in the following format pageNNN.png
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
            {
                return null;
            }

            return new Uri(Path.Combine(location, filename));
        }

        public static bool IncreaseIsolatedStorageSpace(long quotaSizeDemand)
        {
            using (IsolatedStorageFile isolatedStorageFile = IsolatedStorageFile.GetUserStoreForApplication())
            {
                //Get the Available space
                long maxAvailableSpace = isolatedStorageFile.AvailableFreeSpace;

                if (quotaSizeDemand > maxAvailableSpace)
                {
                    return isolatedStorageFile.IncreaseQuotaTo(isolatedStorageFile.Quota + quotaSizeDemand);
                }
                return false;
            }
        }

        public static Uri GetImageFromWeb(string filename, bool useDriveIfExists = true)
        {
            MakeQuranDirectory();
            string path = Path.Combine(GetQuranDirectory(false), filename);
            if (useDriveIfExists && FileExists(path))
            {
                return GetImageFromSD(filename);
            }
            QuranScreenInfo instance = QuranScreenInfo.Instance;
            if (instance == null)
            {
                return null;
            }

            string urlString = Path.Combine(ImgHost + "width" + instance.GetWidthParamWithUnderScore(), filename);
            return new Uri(urlString, UriKind.Absolute);
        }

        public static async Task<bool> DownloadFileFromWebAsync(string uri, string localPath)
        {
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                try
                {
                    var request = (HttpWebRequest) WebRequest.Create(uri);
                    request.Method = HttpMethod.Get;
                    HttpWebResponse response = await request.GetResponseAsync();
                    using (var isfStream = new IsolatedStorageFileStream(localPath, FileMode.Create, isf))
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

        public static string GetQuranDatabaseDirectory(bool asUri, bool createIfDoesntExist = false)
        {
            return GetSubdirectory(asUri, DatabaseDirectory, createIfDoesntExist);
        }

        public static string GetDowloadTrackerDirectory(bool asUri, bool createIfDoesntExist = false)
        {
            return GetSubdirectory(asUri, DownloadsDirectory, createIfDoesntExist);
        }

        public static string GetUndeletedFilesDirectory(bool asUri, bool createIfDoesntExist = false)
        {
            return GetSubdirectory(asUri, UndeletedFilesDirectory, createIfDoesntExist);
        }

        public static string GetSubdirectory(bool asUri, string name, bool createIfDoesntExist = false)
        {
            string baseDir = (asUri ? QuranBaseUri : QuranBase);

            if (createIfDoesntExist)
            {
                string tempPath = Path.Combine(QuranBase, name);
                MakeDirectoryRecursive(tempPath);
            }

            return Path.Combine(baseDir, name);
        }

        public static string GetQuranDirectory(bool asUri, bool createIfDoesntExist = false)
        {
            string baseDir = (asUri ? QuranBaseUri : QuranBase);
            QuranScreenInfo qsi = QuranScreenInfo.Instance;

            if (createIfDoesntExist)
            {
                string tempPath = QuranBase + "width" + qsi.GetWidthParamWithUnderScore();
                MakeDirectoryRecursive(tempPath);
            }

            return Path.Combine(baseDir, "width" + qsi.GetWidthParamWithUnderScore());
        }

        public static string GetZipFileUrl()
        {
            string url = ImgHost;
            QuranScreenInfo qsi = QuranScreenInfo.Instance;
            if (qsi == null)
            {
                return null;
            }
            url += "images" + qsi.GetWidthParamWithUnderScore() + ".zip";
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
                return false;
            }
        }

        public static string GetAyaPositionFileName()
        {
            QuranScreenInfo qsi = QuranScreenInfo.Instance;
            return "ayahinfo" + qsi.GetWidthParamWithUnderScore() + ".db";
        }

        public static string GetAyaPositionFileUrl()
        {
            QuranScreenInfo qsi = QuranScreenInfo.Instance;
            if (qsi == null)
            {
                return null;
            }
            string url = ImgHost + "width" + qsi.GetWidthParamWithUnderScore();
            url += "/ayahinfo" + qsi.GetWidthParamWithUnderScore() + ".zip";
            return url;
        }

        public static string GetGaplessDatabaseRootUrl()
        {
            return ImgHost + DatabaseDirectory + "/" + "audio/";
        }

        public static bool HaveAyaPositionFile()
        {
            string baseDir = GetQuranDatabaseDirectory(false);
            if (baseDir == null)
            {
                MakeQuranDatabaseDirectory();
            }
            string filename = GetAyaPositionFileName();
            if (filename != null)
            {
                string ayaPositionDb = Path.Combine(baseDir, filename);
                if (!FileExists(ayaPositionDb))
                {
                    return false;
                }
                return true;
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
                using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (isf.FileExists(path))
                    {
                        isf.DeleteFile(path);
                    }
                }
                return true;
            }
            return false;
        }

        public static string GetArabicSearchDatabaseUrl()
        {
            return ImgHost + DatabaseDirectory + "/" + QuranArabicDatabase;
        }
    }
}