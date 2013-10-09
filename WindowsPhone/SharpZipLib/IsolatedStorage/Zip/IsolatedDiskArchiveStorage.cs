using System;
using System.IO;
using System.IO.IsolatedStorage;
using QuranPhone.SharpZipLib.Zip;
using QuranPhone.SharpZipLib.IsolatedStorage.Zip;

namespace QuranPhone.SharpZipLib.IsolatedStorage.Zip
{
    /// <summary>
    ///   An <see cref = "IArchiveStorage" /> implementation suitable for hard disks.
    /// </summary>
    public class IsolatedDiskArchiveStorage : BaseArchiveStorage
    {
        #region Fields

        private readonly string fileName_;

        private string temporaryName_;

        private Stream temporaryStream_;

        #endregion

        #region Constructors

        /// <summary>
        ///   Initializes a new instance of the <see cref = "DiskArchiveStorage" /> class.
        /// </summary>
        /// <param name = "file">The file.</param>
        public IsolatedDiskArchiveStorage(IsolatedZipFile file)
            : this(file, FileUpdateMode.Safe)
        {
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref = "DiskArchiveStorage" /> class.
        /// </summary>
        /// <param name = "file">The file.</param>
        /// <param name = "updateMode">The update mode.</param>
        public IsolatedDiskArchiveStorage(IsolatedZipFile file, FileUpdateMode updateMode)
            : base(updateMode)
        {
            if (file.Name == null)
            {
                throw new ZipException("Cant handle non file archives");
            }

            fileName_ = file.Name;
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///   Converts a temporary <see cref = "Stream" /> to its final form.
        /// </summary>
        /// <returns>Returns a <see cref = "Stream" /> that can be used to read
        ///   the final storage for the archive.</returns>
        public override Stream ConvertTemporaryToFinal()
        {
            if (temporaryStream_ == null)
            {
                throw new ZipException("No temporary stream has been created");
            }

            Stream result;

            string moveTempName = GetTempFileName(fileName_, false);
            bool newFileCreated = false;
            IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();
            try
            {
                temporaryStream_.Close();
                store.MoveFile(fileName_, moveTempName);
                store.MoveFile(temporaryName_, fileName_);
                newFileCreated = true;
                store.DeleteFile(moveTempName);

                result = store.OpenFile(fileName_, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            catch (Exception)
            {
                // Try to roll back changes...
                if (!newFileCreated)
                {
                    store.MoveFile(moveTempName, fileName_);
                    store.DeleteFile(temporaryName_);
                }

                throw;
            }

            return result;
        }

        /// <summary>
        ///   Disposes this instance.
        /// </summary>
        public override void Dispose()
        {
            if (temporaryStream_ != null)
            {
                temporaryStream_.Close();
            }
        }

        /// <summary>
        ///   Gets a temporary output <see cref = "Stream" /> for performing updates on.
        /// </summary>
        /// <returns>Returns the temporary output stream.</returns>
        public override Stream GetTemporaryOutput()
        {
            if (temporaryName_ != null)
            {
                temporaryName_ = GetTempFileName(temporaryName_, true);
                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    temporaryStream_ = store.OpenFile(temporaryName_, FileMode.OpenOrCreate, FileAccess.Write,
                                                      FileShare.None);
                }
            }
            else
            {
                // Determine where to place files based on internal strategy.
                // Currently this is always done in system temp directory.
                temporaryName_ = Guid.NewGuid().ToString("N");
                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    temporaryStream_ = store.OpenFile(temporaryName_, FileMode.OpenOrCreate, FileAccess.Write,
                                                      FileShare.None);
                }
            }

            return temporaryStream_;
        }

        /// <summary>
        ///   Make a temporary copy of a stream.
        /// </summary>
        /// <param name = "stream">The <see cref = "Stream" /> to copy.</param>
        /// <returns>Returns a temporary output <see cref = "Stream" /> that is a copy of the input.</returns>
        public override Stream MakeTemporaryCopy(Stream stream)
        {
            stream.Close();

            temporaryName_ = GetTempFileName(fileName_, true);

            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                store.CopyFile(fileName_, temporaryName_);

                temporaryStream_ = store.OpenFile(temporaryName_, FileMode.Open, FileAccess.ReadWrite);
            }

            return temporaryStream_;
        }

        /// <summary>
        ///   Return a stream suitable for performing direct updates on the original source.
        /// </summary>
        /// <param name = "stream">The current stream.</param>
        /// <returns>Returns a stream suitable for direct updating.</returns>
        /// <remarks>
        ///   If the <paramref name = "current" /> stream is not null this is used as is.
        /// </remarks>
        public override Stream OpenForDirectUpdate(Stream stream)
        {
            Stream result;

            if ((stream == null) || !stream.CanWrite)
            {
                if (stream != null)
                {
                    stream.Close();
                }

                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    result = store.OpenFile(fileName_, FileMode.Open, FileAccess.ReadWrite);
                }
            }
            else
            {
                result = stream;
            }

            return result;
        }

        #endregion

        #region Private Methods

        private static string GetTempFileName(string original, bool makeTempFile)
        {
            string result = null;

            if (original == null)
            {
                result = Guid.NewGuid().ToString("N");
            }
            else
            {
                int counter = 0;
                int suffixSeed = DateTime.Now.Second;
                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    while (result == null)
                    {
                        counter += 1;
                        string newName = string.Format("{0}.{1}{2}.tmp", original, suffixSeed, counter);
                        if (!store.FileExists(newName))
                        {
                            if (makeTempFile)
                            {
                                try
                                {
                                    // Try and create the file.
                                    using (store.CreateFile(newName))
                                    {
                                    }
                                    result = newName;
                                }
                                catch
                                {
                                    suffixSeed = DateTime.Now.Second;
                                }
                            }
                            else
                            {
                                result = newName;
                            }
                        }
                    }
                }
            }
            return result;
        }

        #endregion

    }
}
