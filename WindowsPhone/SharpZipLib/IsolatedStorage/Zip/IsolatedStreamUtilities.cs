using System;
using System.IO;
using System.IO.IsolatedStorage;

namespace QuranPhone.SharpZipLib.IsolatedStorage.Zip
{
    public static class IsolatedStreamUtilities
    {
        #region Public Methods

        public static void CopyFile(this IsolatedStorageFile store, string source, string target)
        {
            using (var src = store.OpenFile(source, FileMode.Open))
            {
                using (var dst = store.CreateFile(target))
                {
                    src.CopyTo(dst);
                }
            }
        }


        public static long GetFilesize(string filename)
        {
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                return store.GetFileSize(filename);
            }
        }

        public static long GetFileSize(this IsolatedStorageFile store, string filename)
        {
            using (var fs = store.OpenFile(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return fs.Length;
            }
        }

        public static void CopyTo(this MemoryStream src, Stream dest)
        {
            dest.Write(src.GetBuffer(), (int) src.Position, (int) (src.Length - src.Position));
        }

        public static void CopyTo(this Stream src, MemoryStream dest)
        {
            if (src.CanSeek)
            {
                int pos = (int) dest.Position;
                int length = (int) (src.Length - src.Position) + pos;
                dest.SetLength(length);

                while (pos < length)
                    pos += src.Read(dest.GetBuffer(), pos, length - pos);
            }
            else
                src.CopyTo(dest);
        }

        public static void CopyTo(this Stream src, Stream dest)
        {
            int size = (src.CanSeek) ? Math.Min((int) (src.Length - src.Position), 0x2000) : 0x2000;
            byte[] buffer = new byte[size];
            int n;
            do
            {
                n = src.Read(buffer, 0, buffer.Length);
                dest.Write(buffer, 0, n);
            } while (n != 0);
        }

        public static void MoveFile(this IsolatedStorageFile store, string source, string target)
        {
            CopyFile(store, source, target);

            store.DeleteFile(source);
        }

        #endregion
    }
}
