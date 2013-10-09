using System.IO;
using System.IO.IsolatedStorage;
using QuranPhone.SharpZipLib.Zip;

namespace QuranPhone.SharpZipLib.IsolatedStorage.Zip
{
    /// <summary>
    ///   Default implementation of <see cref = "IDynamicDataSource" /> for files stored on disk.
    /// </summary>
    public class IsolatedDynamicDiskDataSource : IDynamicDataSource
    {

        #region Public Methods

        /// <summary>
        ///   Get a <see cref = "Stream" /> providing data for an entry.
        /// </summary>
        /// <param name = "entry">The entry to provide data for.</param>
        /// <param name = "name">The file name for data if known.</param>
        /// <returns>Returns a stream providing data; or null if not available</returns>
        public Stream GetSource(ZipEntry entry, string name)
        {
            Stream result = null;

            if (name != null)
            {
                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    result = store.OpenFile(name, FileMode.Open, FileAccess.Read, FileShare.Read);
                }
            }

            return result;
        }

        #endregion

    }
}
