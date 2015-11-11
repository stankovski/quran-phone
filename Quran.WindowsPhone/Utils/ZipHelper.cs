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
        public static Task Unzip(string zipPath, string baseFolder)
        {
            //zipPath = FileUtils.Combine(QuranApp.NativeProvider.NativePath, zipPath);
            //baseFolder = FileUtils.Combine(QuranApp.NativeProvider.NativePath, baseFolder);
            return Task.Run(() =>
            {
                using (var isf = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (var fileStream = new IsolatedStorageFileStream(zipPath, FileMode.Open, isf))
                    {
                        UnzipFilesFromStream(fileStream, baseFolder);
                    }
                }
            });
        }

        public static void UnzipFromByteArray(byte[] zipData, string baseFolder)
        {
            baseFolder = FileUtils.Combine(QuranApp.NativeProvider.NativePath, baseFolder);

            using (MemoryStream memoryStream = new MemoryStream(zipData))
            {
                UnzipFilesFromStream(memoryStream, baseFolder);
            }
        }

        private static void UnzipFilesFromStream(Stream source, string baseFolder)
        {
            FileUtils.MakeDirectory(baseFolder);

            using (var isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (ZipArchive package = new ZipArchive(source))
                {
                    foreach (var zipPart in package.Entries)
                    {
                        string path = PathHelper.Combine(baseFolder, zipPart.FullName);

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
