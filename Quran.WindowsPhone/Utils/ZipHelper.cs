using System.IO;
using System.IO.Compression;
using System.IO.IsolatedStorage;
using System.Threading.Tasks;
using Quran.Core;
using Quran.Core.Utils;

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
            await FileUtils.EnsureDirectoryExists(baseFolder);

            using (var isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (ZipArchive package = new ZipArchive(source))
                {
                    foreach (var zipPart in package.Entries)
                    {
                        string path = Path.Combine(baseFolder, zipPart.FullName);

                        if (isf.FileExists(path) || isf.DirectoryExists(path))
                            continue;

                        using (Stream zipStream = zipPart.Open())
                        {
                            using (var fileStream = new IsolatedStorageFileStream(path, FileMode.Create, isf))
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
