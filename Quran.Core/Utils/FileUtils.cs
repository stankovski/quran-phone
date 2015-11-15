using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;
using System.Threading;
using Windows.Storage;
using System.Collections.Generic;

namespace Quran.Core.Utils
{
    public static class FileUtils
    {
        public static bool failedToWrite = false;
        public static string IMG_HOST = "http://android.quran.com/data/";
        public static string QURAN_BASE = "quran_android";
        private static string AUDIO_DIRECTORY = "audio";
        private static string DATABASE_DIRECTORY = "databases";
        private static string DOWNLOADS_DIRECTORY = "downloads";
        private static string UNDELETED_FILES_DIRECTORY = "to-delete";
        public static string PACKAGE_NAME = "com.quran.labs.androidquran";
        public static string QURAN_ARABIC_DATABASE = "quran.ar.db";

        private static bool initialized = false;
        static FileUtils()
        {
            if (initialized)
                return;

            initialized = true;

            // Initialize directory
            FileUtils.RunSync(() => MakeQuranDirectory());
            FileUtils.RunSync(() => MakeQuranDatabaseDirectory());

            // Delete stuck files
            FileUtils.RunSync(() => DeleteStuckFiles());
        }

        public static bool IsSeparator(this char c)
        {
            return c == '\\' || c == '/';
        }

        
        /// <summary>
        /// Deletes folder even if it contains read only files
        /// </summary>
        /// <param name="path"></param>
        public async static Task<bool> DeleteFolder(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            var folder = await StorageFolder.GetFolderFromPathAsync(path);
            foreach (var file in await folder.GetFilesAsync())
            {
                await DeleteFile(file.Path);
            }
            await folder.DeleteAsync();
            return true;
        }

        public async static Task<bool> DeleteFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            try
            {
                var file = await StorageFile.GetFileFromPathAsync(path);
                await file.DeleteAsync();
            }
            catch (FileNotFoundException)
            {
                // Ignore
            }
            catch (Exception)
            { 
                var tempPath = await GetUndeletedFilesDirectory();
                await WriteFile(Path.Combine(tempPath, string.Format("{0}.txt", Guid.NewGuid())), path);
                return false;
            }
            return true;
        }

        public static async Task DeleteStuckFiles()
        {
            var path = await GetUndeletedFilesDirectory();
            try
            {
                var folder = await StorageFolder.GetFolderFromPathAsync(path);
                foreach (var fileName in await folder.GetFilesAsync())
                {
                    try
                    {
                        var badFilePath = await ReadFile(fileName.Path);
                        await DeleteFile(badFilePath);
                        await DeleteFile(fileName.Path);
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

        public static async Task<bool> FileExists(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            try
            {
                await StorageFile.GetFileFromPathAsync(path);
                return true;
            }
            catch (FileNotFoundException)
            {
                return false;
            }
        }

        public static async Task MoveFile(string from, string to)
        {
            if (!string.IsNullOrWhiteSpace(from) &&
                !string.IsNullOrWhiteSpace(to))
            {
                var file = await StorageFile.GetFileFromPathAsync(from);
                var destinationDirectory = await StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(to));
                await file.MoveAsync(destinationDirectory, Path.GetFileName(to), NameCollisionOption.ReplaceExisting);
            }
        }

        public static async Task<bool> DirectoryExists(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            try
            {
                await StorageFolder.GetFolderFromPathAsync(path);
                return true;
            }
            catch (FileNotFoundException)
            {
                return false;
            }
        }

        public static async Task EnsureDirectoryExists(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("path");
            }

            path = path.Replace('/', '\\').TrimEnd('\\');
            StorageFolder baseFolder = GetBaseFolder(path);
            if (baseFolder == null)
            {
                throw new NotSupportedException("This method implementation doesn't support " +
                    "parameters outside paths accessible by ApplicationData.");
            }
            path = path.Substring(baseFolder.Path.Length + 1);

            string[] folderNames = path.Split('\\');
            for (int i = 0; i < folderNames.Length; i++)
            {
                var task = await baseFolder.CreateFolderAsync(folderNames[i], CreationCollisionOption.OpenIfExists);
                baseFolder = task;
            }
        }

        private static StorageFolder GetBaseFolder(string path)
        {
            foreach (var folder in new StorageFolder[] {
                ApplicationData.Current.LocalFolder,
                ApplicationData.Current.RoamingFolder,
                ApplicationData.Current.TemporaryFolder })
            {
                if (path.Contains(folder.Path))
                {
                    return folder;
                }
            }
            return null;
        }

        /// <summary>
        /// Creates Quran root directory and writes no-media file in it
        /// </summary>
        /// <returns></returns>
        public static Task MakeQuranDirectory()
        {
            return GetQuranDirectory();
        }

        public static async Task WriteFile(string path, string content)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            var baseFolderPath = Path.GetDirectoryName(path);
            await EnsureDirectoryExists(baseFolderPath);
            var baseFolder = await StorageFolder.GetFolderFromPathAsync(baseFolderPath);
            var newFile = await baseFolder.CreateFileAsync(Path.GetFileName(path),
                CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(newFile, content);
        }

        public static async Task<string> ReadFile(string path)
        {
            try
            {
                var file = await StorageFile.GetFileFromPathAsync(path);
                return await FileIO.ReadTextAsync(file);
            }
            catch
            {
                return string.Empty;
            }
        }

        public static async Task WriteNoMediaFile()
        {
            await WriteFile(Path.Combine(await GetQuranDirectory(), "/.nomedia"), " ");
        }

        public static async Task<bool> MediaFileExists()
        {
            return await FileExists(Path.Combine(await GetQuranDirectory(), "/.nomedia"));
        }

        public static async Task DeleteNoMediaFile()
        {
            await DeleteFile(Path.Combine(await GetQuranDirectory(), "/.nomedia"));
        }

        /// <summary>
        /// Creates Quran DB directory and writes no-media file in it
        /// </summary>
        /// <returns></returns>
        public static Task MakeQuranDatabaseDirectory()
        {
            return GetQuranDatabaseDirectory();
        }

        public static async Task<bool> HaveAllImages()
        {
            var quranFolderPath = await GetQuranDirectory();
            var quranFolder = await StorageFolder.GetFolderFromPathAsync(quranFolderPath);
            var imageFiles = await quranFolder.GetFilesAsync();
            // Should have at least 95% of pages; of more than that it's not efficient to download the ZIP
            if (imageFiles.Count >= 600)
            {
                // ideally, we should loop for each page and ensure
                // all pages are there, but this will do for now.
                return true;
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

        public static async Task<Uri> GetImageFromStorage(string filename)
        {
            string location = await GetQuranDirectory();
            var quranFolder = await StorageFolder.GetFolderFromPathAsync(location);
            var image = await quranFolder.TryGetItemAsync(filename);
            if (image == null)
            {
                return null;
            }
            else
            {
                return new Uri(image.Path);
            }
        }

        public static async Task<Uri> GetImageFromWeb(string filename)
        {
            var localUri = await GetImageFromStorage(filename);
            if (localUri != null)
            {
                return localUri;
            }
            else
            {
                ScreenUtils instance = ScreenUtils.Instance;
                if (instance == null) return null;

                string urlString = Path.Combine(IMG_HOST + "width" + instance.GetWidthParam(),
                                                filename);
                return new Uri(urlString, UriKind.Absolute);                
            }
        }

        public static async Task<bool> DownloadFileFromWebAsync(string uri, string localPath, 
            CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(uri);
                request.Method = HttpMethod.Get;
                var response = await request.GetResponseAsync();

                await EnsureDirectoryExists(Path.GetDirectoryName(localPath));
                var localFolder = await StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(localPath));
                var localFile = await localFolder.CreateFileAsync(Path.GetFileName(localPath), CreationCollisionOption.ReplaceExisting);
                using (var writeStream = await localFile.OpenStreamForWriteAsync())
                {
                    response.GetResponseStream().CopyTo(writeStream);
                }
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static Task<string> GetQuranAudioDirectory()
        {
            return GetSubdirectory(AUDIO_DIRECTORY);
        }
        
        public static Task<string> GetQuranDatabaseDirectory()
        {
            return GetSubdirectory(DATABASE_DIRECTORY);
        }

        public static Task<string> GetDowloadTrackerDirectory()
        {
            return GetSubdirectory(DOWNLOADS_DIRECTORY);
        }

        public static Task<string> GetUndeletedFilesDirectory()
        {
            return GetSubdirectory(UNDELETED_FILES_DIRECTORY);
        }

        public static async Task<string> GetQuranDirectory()
        {
            ScreenUtils qsi = ScreenUtils.Instance;
            var imageFolder = Path.Combine(QURAN_BASE, "width" + qsi.GetWidthParam());
            if (qsi == null)
            {
                return null;
            }

            return await GetSubdirectory(imageFolder);
        }

        public static async Task<string> GetSubdirectory(string subdirectoryName)
        {
            var baseFolder = ApplicationData.Current.LocalFolder;
            var subFolder = await baseFolder.CreateFolderAsync(subdirectoryName, CreationCollisionOption.OpenIfExists);
            return subFolder.Path;
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

        public static async Task<bool> ExtractZipFile(string source, string baseFolder)
        {
            try
            {
                await QuranApp.NativeProvider.ExtractZip(source, baseFolder);
                return true;
            }
            catch (Exception)
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

        public static async Task<bool> HaveAyaPositionFile()
        {
            string baseDir = await GetQuranDatabaseDirectory();
            string ayaPositionDb = GetAyaPositionFileName();
            
            return await FileExists(Path.Combine(baseDir, ayaPositionDb));
        }

        public static async Task<bool> HaveArabicSearchFile()
        {
            string baseDir = await GetQuranDatabaseDirectory();
            string arabicSearchDb = QURAN_ARABIC_DATABASE;

            return await FileExists(Path.Combine(baseDir, arabicSearchDb));
        }

        public static async Task<bool> HasTranslation(string fileName)
        {
            string baseDir = await GetQuranDatabaseDirectory();

            return await FileExists(Path.Combine(baseDir, fileName));
        }

        public static async Task RemoveTranslation(string fileName)
        {
            string baseDir = await GetQuranDatabaseDirectory();
            await DeleteFile(Path.Combine(baseDir, fileName));
        }

        public static string GetArabicSearchUrl()
        {
            return IMG_HOST + DATABASE_DIRECTORY + "/" + QURAN_ARABIC_DATABASE;
        }

        public static async Task<bool> IsFileEmpty(string localFilePath)
        {
            try
            {
                var content = await ReadFile(localFilePath);
                return string.IsNullOrWhiteSpace(content);
            }
            catch
            {
                return true;
            }
        }

        /// <summary>
        /// Execute's an async Task<T> method which has a void return value synchronously
        /// </summary>
        /// <param name="task">Task<T> method to execute</param>
        public static void RunSync(Func<Task> task)
        {
            var oldContext = SynchronizationContext.Current;
            var synch = new ExclusiveSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(synch);
            synch.Post(async _ =>
            {
                try
                {
                    await task();
                }
                catch (Exception e)
                {
                    synch.InnerException = e;
                    throw;
                }
                finally
                {
                    synch.EndMessageLoop();
                }
            }, null);
            synch.BeginMessageLoop();

            SynchronizationContext.SetSynchronizationContext(oldContext);
        }

        /// <summary>
        /// Execute's an async Task<T> method which has a T return type synchronously
        /// </summary>
        /// <typeparam name="T">Return Type</typeparam>
        /// <param name="task">Task<T> method to execute</param>
        /// <returns></returns>
        public static T RunSync<T>(Func<Task<T>> task)
        {
            var oldContext = SynchronizationContext.Current;
            var synch = new ExclusiveSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(synch);
            T ret = default(T);
            synch.Post(async _ =>
            {
                try
                {
                    ret = await task();
                }
                catch (Exception e)
                {
                    synch.InnerException = e;
                    throw;
                }
                finally
                {
                    synch.EndMessageLoop();
                }
            }, null);
            synch.BeginMessageLoop();
            SynchronizationContext.SetSynchronizationContext(oldContext);
            return ret;
        }

        private class ExclusiveSynchronizationContext : SynchronizationContext
        {
            private bool done;
            public Exception InnerException { get; set; }
            readonly AutoResetEvent workItemsWaiting = new AutoResetEvent(false);
            readonly Queue<Tuple<SendOrPostCallback, object>> items =
                new Queue<Tuple<SendOrPostCallback, object>>();

            public override void Send(SendOrPostCallback d, object state)
            {
                throw new NotSupportedException("We cannot send to our same thread");
            }

            public override void Post(SendOrPostCallback d, object state)
            {
                lock (items)
                {
                    items.Enqueue(Tuple.Create(d, state));
                }
                workItemsWaiting.Set();
            }

            public void EndMessageLoop()
            {
                Post(_ => done = true, null);
            }

            public void BeginMessageLoop()
            {
                while (!done)
                {
                    Tuple<SendOrPostCallback, object> task = null;
                    lock (items)
                    {
                        if (items.Count > 0)
                        {
                            task = items.Dequeue();
                        }
                    }
                    if (task != null)
                    {
                        task.Item1(task.Item2);
                        if (InnerException != null) // the method threw an exeption
                        {
                            throw new AggregateException("AsyncHelpers.Run method threw an exception.", InnerException);
                        }
                    }
                    else
                    {
                        workItemsWaiting.WaitOne();
                    }
                }
            }

            public override SynchronizationContext CreateCopy()
            {
                return this;
            }
        }
    }
}
