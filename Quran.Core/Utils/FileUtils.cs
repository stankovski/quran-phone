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
using Windows.Storage.Search;
using Quran.Core.ViewModels;
using System.Net.Http;
using Microsoft.ApplicationInsights;
using Windows.ApplicationModel;

namespace Quran.Core.Utils
{
    public static class FileUtils
    {
        private static bool failedToWrite = false;
        private static string QuranImagesUrl = "http://android.quran.com/data/";
        private static string QuranBase = "QuranWindows";
        private static string DatabaseDirectory = "databases";
        private static string UndeletedFilesDirectory = "to-delete";
        private const string QuranDatabaseName = "quran.ar.db";
        private static TelemetryClient telemetry = new TelemetryClient();

        private static bool initialized = false;

        public static StorageFolder BaseFolder { get; private set; }

        public static StorageFolder ImageFolder { get; private set; }
        
        public static StorageFolder DatabaseFolder { get; private set; }

        public static StorageFile ArabicDatabase { get; private set; }

        public static StorageFolder AudioFolder { get; private set; }

        public static ScreenInfo ScreenInfo { get; private set; }

        public async static Task Initialize(ScreenInfo screenInfo)
        {
            if (initialized)
                return;

            initialized = true;
            ScreenInfo = screenInfo;

            // Initialize directory
            await MakeQuranDirectory(screenInfo);
            await MakeQuranDatabaseDirectory();
            await MakeQuranAudioDirectory();
            // Delete stuck files
            await DeleteStuckFiles();
            
        }
        
        /// <summary>
        /// Deletes folder even if it contains read only files
        /// </summary>
        /// <param name="path"></param>
        public async static Task<bool> DeleteFolder(StorageFolder baseDir, string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            var folder = await baseDir.TryGetItemAsync(path) as StorageFolder;
            if (folder == null)
            {
                return false;
            }

            foreach (var file in await folder.GetFilesAsync())
            {
                await SafeFileDelete(file.Path);
            }
            await folder.DeleteAsync();
            return true;
        }

        public async static Task<bool> SafeFileDelete(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            try
            {
                var file = await StorageFile.GetFileFromPathAsync(path);
                return await SafeFileDelete(file);
            }
            catch (FileNotFoundException)
            {
                // Ignore
                return false;
            }            
        }

        public async static Task<bool> SafeFileDelete(StorageFile file)
        {
            if (file == null)
            {
                return false;
            }

            try
            {
                await file.DeleteAsync();
            }
            catch (FileNotFoundException)
            {
                // Ignore
            }
            catch (Exception)
            {
                var tempPath = GetUndeletedFilesDirectory();
                await WriteFile(Path.Combine(tempPath, string.Format("{0}.txt", Guid.NewGuid())), file.Path);
                return false;
            }
            return true;
        }

        public static async Task DeleteStuckFiles()
        {
            var undeletedDir = GetUndeletedFilesDirectory();
            try
            {
                // Remove undeleted files
                var undeletedFolder = await StorageFolder.GetFolderFromPathAsync(undeletedDir);
                foreach (var storageFile in await undeletedFolder.GetFilesAsync())
                {
                    try
                    {
                        var badFilePath = await FileIO.ReadTextAsync(storageFile);
                        await SafeFileDelete(badFilePath);
                        await SafeFileDelete(storageFile);
                    }
                    catch
                    {
                        // Continue
                    }
                }

                // Remove failed downloads
                var queryOptions = new QueryOptions(CommonFileQuery.DefaultQuery, new[] { DownloadableViewModelBase.DownloadExtension });
                queryOptions.FolderDepth = FolderDepth.Deep;

                var storageFiles = (await BaseFolder.CreateFileQueryWithOptions(queryOptions).GetFilesAsync())
                                    .Concat(await AudioFolder.CreateFileQueryWithOptions(queryOptions).GetFilesAsync());
                foreach (var storageFile in storageFiles)
                {
                    try
                    {
                        await SafeFileDelete(storageFile);
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
                return await GetFile(path) != null;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static async Task<bool> FileExists(StorageFolder baseFolder, string fileName)
        {
            if (baseFolder == null || string.IsNullOrWhiteSpace(fileName))
            {
                return false;
            }

            try
            {
                var storageItem = await baseFolder.TryGetItemAsync(fileName);
                return storageItem != null && storageItem.IsOfType(StorageItemTypes.File);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static async Task<IStorageFile> GetFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            try
            {
                var dirName = Path.GetDirectoryName(path);
                var folder = await StorageFolder.GetFolderFromPathAsync(dirName);
                return await GetFile(folder, Path.GetFileName(path));
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static async Task<StorageFile> GetFile(StorageFolder folder, string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            try
            {
                return await folder.TryGetItemAsync(Path.GetFileName(path)) as StorageFile;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static async Task MoveFile(IStorageFile file, IStorageFolder destinationDirectory, string newName)
        {
            if (file != null && destinationDirectory != null && !string.IsNullOrWhiteSpace(newName))
            {
                await file.MoveAsync(destinationDirectory, newName, NameCollisionOption.ReplaceExisting);
            }
        }

        public static async Task<bool> DirectoryExists(StorageFolder baseDir, string path)
        {
            try
            {
                return (await baseDir.TryGetItemAsync(path)) != null;
            }
            catch (FileNotFoundException)
            {
                return false;
            }
        }

        public static Task EnsureDirectoryExists(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            return Task.Run(() => Directory.CreateDirectory(path));
        }

        /// <summary>
        /// Creates Quran root directory.
        /// </summary>
        /// <returns></returns>
        public async static Task MakeQuranDirectory(ScreenInfo qsi)
        {
            if (qsi == null)
            {
                throw new ArgumentNullException(nameof(qsi));
            }

            if (BaseFolder == null)
            {
                BaseFolder = ApplicationData.Current.LocalFolder;
                var quranBaseFolder = await BaseFolder.TryGetItemAsync(QuranBase);
                if (quranBaseFolder == null)
                {
                    quranBaseFolder = await BaseFolder.CreateFolderAsync(QuranBase);
                }
                BaseFolder = quranBaseFolder as StorageFolder;
            }

            if (BaseFolder == null)
            {
                throw new InvalidOperationException("Unable to create a base folder.");
            }

            if (ImageFolder == null)
            {
                var imageFolderName = "width" + qsi.GetWidthParam();
                var quranBaseFolder = await BaseFolder.TryGetItemAsync(imageFolderName);
                if (quranBaseFolder == null)
                {
                    quranBaseFolder = await BaseFolder.CreateFolderAsync(imageFolderName);
                }
                ImageFolder = quranBaseFolder as StorageFolder;
            }

            if (ImageFolder == null)
            {
                throw new InvalidOperationException("Unable to create a base image folder.");
            }
        }

        /// <summary>
        /// Creates Quran DB directory.
        /// </summary>
        /// <returns></returns>
        public static async Task MakeQuranDatabaseDirectory()
        {
            if (BaseFolder == null)
            {
                throw new ArgumentNullException("BaseFolder");
            }

            if (DatabaseFolder == null)
            {
                var databaseFolder = await BaseFolder.TryGetItemAsync(DatabaseDirectory);
                if (databaseFolder == null)
                {
                    databaseFolder = await BaseFolder.CreateFolderAsync(DatabaseDirectory);
                }
                DatabaseFolder = databaseFolder as StorageFolder;
            }

            if (DatabaseFolder == null)
            {
                throw new InvalidOperationException("Unable to create a database folder.");
            }

            StorageFolder installationFolder = Package.Current.InstalledLocation;
            StorageFolder assetsFolder = await installationFolder.GetFolderAsync("Assets");
            StorageFile arabicDatabase = await assetsFolder.GetFileAsync(QuranDatabaseName);
            await arabicDatabase.CopyAsync(DatabaseFolder, QuranDatabaseName, NameCollisionOption.ReplaceExisting);
            ArabicDatabase = await DatabaseFolder.GetFileAsync(QuranDatabaseName);
        }

        /// <summary>
        /// Creates Quran root audio directory.
        /// </summary>
        /// <returns></returns>
        public async static Task MakeQuranAudioDirectory()
        {
            if (AudioFolder == null)
            {
                var baseFolder = KnownFolders.MusicLibrary;
                var quranAudioFolder = await baseFolder.TryGetItemAsync(QuranBase);
                if (quranAudioFolder == null)
                {
                    quranAudioFolder = await baseFolder.CreateFolderAsync(QuranBase);
                }
                AudioFolder = quranAudioFolder as StorageFolder;
            }

            if (AudioFolder == null)
            {
                throw new InvalidOperationException("Unable to create a audio folder.");
            }
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

        public static async Task<bool> HaveAllImages()
        {
            var imageFiles = ImageFolder.CreateFileQuery();
            // Should have at least 95% of pages; of more than that it's not efficient to download the ZIP
            var pageCount = await imageFiles.GetItemCountAsync(); 
            if (pageCount >= 600)
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

        public static Uri GetImageOnlineUri(string filename)
        {
            if (ScreenInfo == null) return null;
            string urlString = Path.Combine(QuranImagesUrl + "width" + ScreenInfo.GetWidthParam(),
                                            filename);
            return new Uri(urlString, UriKind.Absolute);
        }


        //public static Task<bool> DownloadFileFromWebAsync(string uri, StorageFile destination,
        //    CancellationToken cancellationToken = default(CancellationToken))
        //{
        //    using (var httpClient = new HttpClient())
        //    {
        //        return DownloadFileFromWebAsync(httpClient, uri, destination, cancellationToken);
        //    }
        //}

        //public static async Task<bool> DownloadFileFromWebAsync(HttpClient httpClient, 
        //    string uri, StorageFile destination,
        //    CancellationToken cancellationToken = default(CancellationToken))
        //{
        //    try
        //    {
        //        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, new Uri(uri));
        //        var response = await httpClient.SendAsync(request);
        //        using (var contentStream = await response.Content.ReadAsStreamAsync())
        //        {
        //            await contentStream.CopyToAsync(await destination.OpenStreamForWriteAsync());
        //        }
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        telemetry.TrackException(ex);
        //        return false;
        //    }
        //}

        public static async Task<bool> DownloadFileFromWebAsync(string uri, string localPath,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(uri);
                request.Method = "GET";
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
        
        public static string GetQuranDatabaseDirectory()
        {
            return DatabaseFolder.Path;
        }

        public static string GetUndeletedFilesDirectory()
        {
            return GetSubdirectory(UndeletedFilesDirectory);
        }

        public static string GetSubdirectory(string subdirectoryName)
        {
            var baseFolder = ApplicationData.Current.LocalFolder;
            var subdirectoryPath = Path.Combine(baseFolder.Path, subdirectoryName);
            Directory.CreateDirectory(subdirectoryPath);
            return subdirectoryPath;
        }

        public static string GetZipFileUrl()
        {
            string url = QuranImagesUrl;
            if (ScreenInfo == null)
            {
                return null;
            }

            url += "images" + ScreenInfo.GetWidthParam() + ".zip";
            return url;
        }

        public static string GetAyaPositionFileName()
        {
            if (ScreenInfo == null)
            {
                return null;
            }
            return "ayahinfo" + ScreenInfo.GetWidthParam() + ".db";
        }

        public static string GetAyaPositionFileUrl()
        {
            if (ScreenInfo == null)
            {
                return null;
            }

            string url = QuranImagesUrl + "width" + ScreenInfo.GetWidthParam();
            url += "/ayahinfo" + ScreenInfo.GetWidthParam() + ".zip";
            return url;
        }
        
        public static async Task<bool> HaveAyaPositionFile()
        {
            string ayaPositionDb = GetAyaPositionFileName();
            
            return await FileExists(DatabaseFolder, ayaPositionDb);
        }
    }
}
