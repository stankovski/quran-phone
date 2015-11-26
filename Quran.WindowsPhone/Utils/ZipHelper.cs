using System;
using System.IO;
using System.IO.Compression;
using System.IO.IsolatedStorage;
using System.Threading.Tasks;
using Quran.Core;
using Quran.Core.Utils;
using Windows.Storage;

namespace Quran.WindowsPhone.Utils
{
    public static class ZipHelper
    {
        public static async Task Unzip(string zipPath, string baseFolder)
        {
            //zipPath = FileUtils.Combine(QuranApp.NativeProvider.NativePath, zipPath);
            //baseFolder = FileUtils.Combine(QuranApp.NativeProvider.NativePath, baseFolder);
            using (var isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (var fileStream = new IsolatedStorageFileStream(zipPath, FileMode.Open, isf))
                {
                    await UnzipFilesFromStream(fileStream, baseFolder);
                }
            }
        }

        public static async Task UnzipFromByteArray(byte[] zipData, string baseFolder)
        {
            baseFolder = Path.Combine(QuranApp.NativeProvider.NativePath, baseFolder);

            using (MemoryStream memoryStream = new MemoryStream(zipData))
            {
                await UnzipFilesFromStream(memoryStream, baseFolder);
            }
        }

        private static async Task UnzipFilesFromStream(Stream source, string baseFolder)
        {
            using (ZipArchive package = new ZipArchive(source))
            {
                foreach (var zipPart in package.Entries)
                {
                    string path = Path.Combine(baseFolder, zipPart.FullName);

                    if (await FileUtils.FileExists(path) || await FileUtils.DirectoryExists(path))
                    {
                        continue;
                    }

                    var zipPartDirPath = Path.GetDirectoryName(path);
                    await FileUtils.EnsureDirectoryExists(zipPartDirPath);

                    if (zipPart.Length > 0)
                    {
                        using (Stream zipStream = zipPart.Open())
                        {
                            var zipPartDir = await StorageFolder.GetFolderFromPathAsync(zipPartDirPath);
                            var zipPartFile = await zipPartDir.CreateFileAsync(Path.GetFileName(path), CreationCollisionOption.ReplaceExisting);

                            using (var fileStream = await zipPartFile.OpenStreamForWriteAsync())
                            {
                                zipStream.CopyTo(fileStream);
                            }
                        }
                    }
                }
            }
        }
    }
}
