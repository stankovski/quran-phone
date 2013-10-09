using System.IO;
using System.IO.IsolatedStorage;
using QuranPhone.SharpZipLib.Zip;

namespace QuranPhone.SharpZipLib.IsolatedStorage.Zip
{
    /// <summary>
    /// Default implementation of a <see cref="IStaticDataSource"/> for use with files stored on disk.
    /// </summary>
    public class IsolatedStaticDiskDataSource : IStaticDataSource
    {
        /// <summary>
        /// Initialise a new instnace of <see cref="StaticDiskDataSource"/>
        /// </summary>
        /// <param name="fileName">The name of the file to obtain data from.</param>
        public IsolatedStaticDiskDataSource(string fileName)
        {
            fileName_ = fileName;
        }

        #region IDataSource Members

        /// <summary>
        /// Get a <see cref="Stream"/> providing data.
        /// </summary>
        /// <returns>Returns a <see cref="Stream"/> provising data.</returns>
        public Stream GetSource()
        {
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                return store.OpenFile(fileName_, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
        }

        #endregion
        #region Instance Fields
        string fileName_;
        #endregion
    }
}