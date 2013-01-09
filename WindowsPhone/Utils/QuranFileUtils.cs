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

namespace QuranPhone.Utils
{
    public class QuranFileUtils
    {

        private const string TAG = "QuranFileUtils";
        public static bool failedToWrite = false;
        public static string IMG_HOST = "http://android.quran.com/data/";
        private static string QURAN_BASE = "isostore:/quran_android" + Path.PathSeparator;
        private static string DATABASE_DIRECTORY = "databases";
        private static int BUFF_SIZE = 1024;
        public const string PACKAGE_NAME = "com.quran.labs.androidquran";

        public static bool HaveAllImages()
        {
            IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication();
            if (isf.DirectoryExists(GetQuranDirectory()))
            {
                var files = isf.GetFileNames(GetQuranDirectory());
                if (files.Length >= 604)
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

            return false;
        }

        public static string GetPageFileName(int p)
        {
            return "page" + p.ToString("000", new CultureInfo("us-US")) + ".png";
        }

        public static Uri GetImageFromSD(string filename)
        {
            string location = GetQuranDirectory();
            if (location == null)
                return null;

            return new Uri(location + Path.PathSeparator + filename);
        }

        public static bool WriteNoMediaFile()
        {
            IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication();
            if (isf.FileExists(GetQuranDirectory() + "/.nomedia"))
                return true;

            try
            {
                using (var stream = isf.CreateFile(GetQuranDirectory() + "/.nomedia"))
                {
                    stream.WriteByte(1);
                }
                return true;
            }
            catch (IOException e)
            {
                return false;
            }
        }

        public static bool MakeQuranDirectory()
        {
            string path = GetQuranDirectory();
            if (path == null)
                return false;

            IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication();

            if (isf.DirectoryExists(path))
            {
                return WriteNoMediaFile();
            }
            else
            {
                try
                {
                    isf.CreateDirectory(path);
                    return WriteNoMediaFile();
                }
                catch
                {
                    return false;
                }
            }
        }

        public static bool MakeQuranDatabaseDirectory()
        {
            string path = GetQuranDatabaseDirectory();
            if (path == null)
                return false;

            IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication();

            if (isf.DirectoryExists(path))
            {
                return WriteNoMediaFile();
            }
            else
            {
                try
                {
                    isf.CreateDirectory(path);
                    return WriteNoMediaFile();
                }
                catch
                {
                    return false;
                }
            }
        }

        private static bool increaseIsolatedStorageSpace(long quotaSizeDemand)
        {
            bool canSizeIncrease = false;
            IsolatedStorageFile isolatedStorageFile = IsolatedStorageFile.GetUserStoreForApplication();

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

        public static Uri GetImageFromWeb(string filename)
        {
            QuranScreenInfo instance = QuranScreenInfo.GetInstance();
            if (instance == null) return null;

            string urlString = IMG_HOST + "width"
                    + instance.GetWidthParam() + "/"
                    + filename;
            Debug.WriteLine("want to download: " + urlString);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlString);
            HttpWebResponse response = null;
            request.Method = HttpMethod.Get;
            request.GetResponseAsync().ContinueWith(result => { response = result.Result; }).Wait();
            using (var sr = response.GetResponseStream())
            {
                string path = GetQuranDirectory();
                if (path != null)
                {
                    path += Path.PathSeparator + filename;

                    try
                    {
                        bool checkQuotaIncrease = QuranFileUtils.increaseIsolatedStorageSpace(response.ContentLength);

                        var isf = IsolatedStorageFile.GetUserStoreForApplication();
                        var isfStream = new IsolatedStorageFileStream(filename, FileMode.Create, isf);
                        long VideoFileLength = (long)response.ContentLength;
                        byte[] byteImage = new byte[VideoFileLength];
                        sr.Read(byteImage, 0, byteImage.Length);
                        isfStream.Write(byteImage, 0, byteImage.Length);
                        return GetImageFromSD(filename);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("failed to store file {0}: {1}", path, e.Message);
                        return new Uri(urlString);
                    }
                }
            }
            return new Uri(urlString);
        }

        public static string GetQuranBaseDirectory()
        {
            return QURAN_BASE;
        }

        public static string GetQuranDatabaseDirectory()
        {
            string baseDir = GetQuranBaseDirectory();
            return (baseDir == null) ? null : baseDir + DATABASE_DIRECTORY;
        }

        public static string GetQuranDirectory()
        {
            string baseDir = GetQuranBaseDirectory();
            QuranScreenInfo qsi = QuranScreenInfo.GetInstance();
            if (qsi == null)
                return null;
            return (baseDir == null) ? null : baseDir + "width" + qsi.GetWidthParam();
        }

        public static string GetZipFileUrl()
        {
            string url = IMG_HOST;
            QuranScreenInfo qsi = QuranScreenInfo.GetInstance();
            if (qsi == null)
                return null;
            url += "images" + qsi.GetWidthParam() + ".zip";
            return url;
        }

        public static string GetAyaPositionFileName()
        {
            QuranScreenInfo qsi = QuranScreenInfo.GetInstance();
            if (qsi == null) return null;
            return "ayahinfo" + qsi.GetWidthParam() + ".db";
        }

        public static string GetAyaPositionFileUrl()
        {
            QuranScreenInfo qsi = QuranScreenInfo.GetInstance();
            if (qsi == null)
                return null;
            string url = IMG_HOST + "width" + qsi.GetWidthParam();
            url += "/ayahinfo" + qsi.GetWidthParam() + ".zip";
            return url;
        }

        public static string GetGaplessDatabaseRootUrl()
        {
            QuranScreenInfo qsi = QuranScreenInfo.GetInstance();
            if (qsi == null)
                return null;
            return IMG_HOST + "databases/audio/";
        }

        public static bool HaveAyaPositionFile()
        {
            string baseDir = QuranFileUtils.GetQuranDatabaseDirectory();
            if (baseDir == null)
                QuranFileUtils.MakeQuranDatabaseDirectory();
            string filename = QuranFileUtils.GetAyaPositionFileName();
            if (filename != null)
            {
                string ayaPositionDb = baseDir + Path.PathSeparator + filename;
                IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication();
                if (!isf.FileExists(ayaPositionDb))
                {
                    return false;
                }
                else { return true; }
            }

            return false;
        }

        public static bool HasTranslation(string fileName)
        {
            string path = GetQuranDatabaseDirectory();
            if (path != null)
            {
                path += Path.PathSeparator + fileName;
                IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication();
                return isf.FileExists(path);
            }
            return false;
        }

        public static bool RemoveTranslation(string fileName)
        {
            string path = GetQuranDatabaseDirectory();
            if (path != null)
            {
                path += Path.PathSeparator + fileName;
                IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication();
                if (isf.FileExists(path))
                    isf.DeleteFile(path);
                return true;
            }
            return false;
        }

        //public static bool hasArabicSearchDatabase(){
        //   return hasTranslation(QuranDataProvider.QURAN_ARABIC_DATABASE);
        //}

        //public static string getArabicSearchDatabaseUrl(){
        //   return IMG_HOST + DATABASE_DIRECTORY + "/" +
        //           QuranDataProvider.QURAN_ARABIC_DATABASE;
        //}   
    }
}
