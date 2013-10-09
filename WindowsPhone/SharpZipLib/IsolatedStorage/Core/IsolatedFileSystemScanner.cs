using System;
using System.IO;
using System.IO.IsolatedStorage;
using QuranPhone.SharpZipLib.Core;
using QuranPhone.SharpZipLib.IsolatedStorage.Core;

namespace QuranPhone.SharpZipLib.IsolatedStorage.Core
{
    /// <summary>
    ///   FileSystemScanner provides facilities scanning of files and directories.
    /// </summary>
    public class IsolatedFileSystemScanner
    {
        #region Fields

        /// <summary>
        ///   Flag indicating if scanning should continue running.
        /// </summary>
        private bool alive_;

        /// <summary>
        ///   Delegate to invoke when processing for a file has finished.
        /// </summary>
        public CompletedFileHandler CompletedFile;

        /// <summary>
        ///   Delegate to invoke when a directory failure is detected.
        /// </summary>
        public DirectoryFailureHandler DirectoryFailure;

        /// <summary>
        ///   The directory filter currently in use.
        /// </summary>
        private readonly IScanFilter directoryFilter_;

        /// <summary>
        ///   Delegate to invoke when a file failure is detected.
        /// </summary>
        public FileFailureHandler FileFailure;

        /// <summary>
        ///   The file filter currently in use.
        /// </summary>
        private readonly IScanFilter fileFilter_;

        /// <summary>
        ///   Delegate to invoke when a directory is processed.
        /// </summary>
        public ProcessDirectoryHandler ProcessDirectory;

        /// <summary>
        ///   Delegate to invoke when a file is processed.
        /// </summary>
        public ProcessFileHandler ProcessFile;

        #endregion

        #region Constructors

        /// <summary>
        ///   Initialise a new instance of <see cref = "FileSystemScanner"></see>
        /// </summary>
        /// <param name = "fileFilter">The file <see cref = "IScanFilter">filter</see>  to apply.</param>
        /// <param name = "directoryFilter">The directory <see cref = "IScanFilter">filter</see>  to apply.</param>
        public IsolatedFileSystemScanner(IScanFilter fileFilter, IScanFilter directoryFilter)
        {
            fileFilter_ = fileFilter;
            directoryFilter_ = directoryFilter;
        }

        /// <summary>
        ///   Initialise a new instance of <see cref = "FileSystemScanner"></see>
        /// </summary>
        /// <param name = "fileFilter">The <see cref = "PathFilter">file filter</see> to apply.</param>
        /// <param name = "directoryFilter">The <see cref = "PathFilter"> directory filter</see> to apply.</param>
        public IsolatedFileSystemScanner(string fileFilter, string directoryFilter)
        {
            fileFilter_ = new IsolatedPathFilter(fileFilter);
            directoryFilter_ = new IsolatedPathFilter(directoryFilter);
        }

        /// <summary>
        ///   Initialise a new instance of <see cref = "FileSystemScanner"></see>
        /// </summary>
        /// <param name = "filter">The <see cref = "PathFilter">file filter</see> to apply when scanning.</param>
        public IsolatedFileSystemScanner(string filter)
        {
            fileFilter_ = new IsolatedPathFilter(filter);
        }

        /// <summary>
        ///   Initialise a new instance of <see cref = "FileSystemScanner"></see>
        /// </summary>
        /// <param name = "fileFilter">The file <see cref = "IScanFilter">filter</see> to apply.</param>
        public IsolatedFileSystemScanner(IScanFilter fileFilter)
        {
            fileFilter_ = fileFilter;
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///   Scan a directory.
        /// </summary>
        /// <param name = "directory">The base directory to scan.</param>
        /// <param name = "recurse">True to recurse subdirectories, false to scan a single directory.</param>
        public void Scan(string directory, bool recurse)
        {
            alive_ = true;
            ScanDir(directory, recurse);
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///   Raise the complete file event
        /// </summary>
        /// <param name = "file">The file name</param>
        private void OnCompleteFile(string file)
        {
            CompletedFileHandler handler = CompletedFile;

            if (handler != null)
            {
                var args = new ScanEventArgs(file);
                handler(this, args);
                alive_ = args.ContinueRunning;
            }
        }

        /// <summary>
        ///   Raise the DirectoryFailure event.
        /// </summary>
        /// <param name = "directory">The directory name.</param>
        /// <param name = "e">The exception detected.</param>
        private bool OnDirectoryFailure(string directory, Exception e)
        {
            DirectoryFailureHandler handler = DirectoryFailure;
            bool result = (handler != null);
            if (result)
            {
                var args = new ScanFailureEventArgs(directory, e);
                handler(this, args);
                alive_ = args.ContinueRunning;
            }
            return result;
        }

        /// <summary>
        ///   Raise the FileFailure event.
        /// </summary>
        /// <param name = "file">The file name.</param>
        /// <param name = "e">The exception detected.</param>
        private bool OnFileFailure(string file, Exception e)
        {
            FileFailureHandler handler = FileFailure;

            bool result = (handler != null);

            if (result)
            {
                var args = new ScanFailureEventArgs(file, e);
                FileFailure(this, args);
                alive_ = args.ContinueRunning;
            }
            return result;
        }

        /// <summary>
        ///   Raise the ProcessDirectory event.
        /// </summary>
        /// <param name = "directory">The directory name.</param>
        /// <param name = "hasMatchingFiles">Flag indicating if the directory has matching files.</param>
        private void OnProcessDirectory(string directory, bool hasMatchingFiles)
        {
            ProcessDirectoryHandler handler = ProcessDirectory;

            if (handler != null)
            {
                var args = new DirectoryEventArgs(directory, hasMatchingFiles);
                handler(this, args);
                alive_ = args.ContinueRunning;
            }
        }

        /// <summary>
        ///   Raise the ProcessFile event.
        /// </summary>
        /// <param name = "file">The file name.</param>
        private void OnProcessFile(string file)
        {
            ProcessFileHandler handler = ProcessFile;

            if (handler != null)
            {
                var args = new ScanEventArgs(file);
                handler(this, args);
                alive_ = args.ContinueRunning;
            }
        }

        private void ScanDir(string directory, bool recurse)
        {
            var store = IsolatedStorageFile.GetUserStoreForApplication();
            try
            {
                string[] names = store.GetFileNames(Path.Combine(directory, "*.*"));

                bool hasMatch = false;
                for (int fileIndex = 0; fileIndex < names.Length; ++fileIndex)
                {
                    if (!fileFilter_.IsMatch(names[fileIndex]))
                    {
                        names[fileIndex] = null;
                    }
                    else
                    {
                        hasMatch = true;
                    }
                }

                OnProcessDirectory(directory, hasMatch);

                if (alive_ && hasMatch)
                {
                    foreach (string fileName in names)
                    {
                        try
                        {
                            if (fileName != null)
                            {
                                OnProcessFile(fileName);
                                if (!alive_)
                                {
                                    break;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            if (!OnFileFailure(fileName, e))
                            {
                                throw;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                if (!OnDirectoryFailure(directory, e))
                {
                    throw;
                }
            }

            if (alive_ && recurse)
            {
                try
                {
                    string[] names = store.GetDirectoryNames(Path.Combine(directory, "*"));

                    foreach (string fulldir in names)
                    {
                        if ((directoryFilter_ == null) || (directoryFilter_.IsMatch(fulldir)))
                        {
                            ScanDir(fulldir, true);
                            if (!alive_)
                            {
                                break;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    if (!OnDirectoryFailure(directory, e))
                    {
                        throw;
                    }
                }
            }
        }

        #endregion

    }
}
