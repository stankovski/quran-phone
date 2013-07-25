using System.IO;
using System.Linq;
using System.IO.IsolatedStorage;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using QuranPhone.Utils;

namespace Tests
{
    [TestClass]
    public class TestUtils
    {
        //[TestMethod]
        //public void TestMakeQuranDir()
        //{
        //    // Setup
        //    IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication();
        //    if (isf.DirectoryExists(QuranFileUtils.GetQuranDirectory(false)))
        //        QuranFileUtils.DeleteFolder((QuranFileUtils.GetQuranDirectory(false)));
        //    Assert.IsFalse(isf.DirectoryExists(QuranFileUtils.GetQuranDirectory(false)));

        //    // Act
        //    Assert.IsTrue(QuranFileUtils.MakeQuranDirectory());

        //    // Verify
        //    Assert.IsTrue(isf.DirectoryExists(QuranFileUtils.GetQuranDirectory(false)));

        //    // Cleanup
        //    QuranFileUtils.DeleteFolder((QuranFileUtils.GetQuranDirectory(false)));
        //}

        //[TestMethod]
        //public void TestMakeQuranDbDir()
        //{
        //    // Setup
        //    IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication();
        //    if (isf.DirectoryExists(QuranFileUtils.GetQuranDatabaseDirectory(false)))
        //        QuranFileUtils.DeleteFolder((QuranFileUtils.GetQuranDatabaseDirectory(false)));
        //    Assert.IsFalse(isf.DirectoryExists(QuranFileUtils.GetQuranDatabaseDirectory(false)));

        //    // Act
        //    Assert.IsTrue(QuranFileUtils.MakeQuranDatabaseDirectory());

        //    // Verify
        //    Assert.IsTrue(isf.DirectoryExists(QuranFileUtils.GetQuranDatabaseDirectory(false)));

        //    // Cleanup
        //    QuranFileUtils.DeleteFolder((QuranFileUtils.GetQuranDatabaseDirectory(false)));
        //}

        [TestMethod]
        public void TestGetPageFileName()
        {
            Assert.AreEqual("page000.png", QuranFileUtils.GetPageFileName(0));
            Assert.AreEqual("page002.png", QuranFileUtils.GetPageFileName(2));
            Assert.AreEqual("page055.png", QuranFileUtils.GetPageFileName(55));
            Assert.AreEqual("page150.png", QuranFileUtils.GetPageFileName(150));
            Assert.AreEqual("page999.png", QuranFileUtils.GetPageFileName(999));
        }

        [TestMethod]
        public void TestGetImageFromWeb()
        {
            Assert.IsTrue(QuranFileUtils.GetImageFromWeb(QuranFileUtils.GetPageFileName(55)) != null);
        }

        [TestMethod]
        public void VerifyHashNotNullAndConsistent()
        {
            var hash = CryptoUtils.GetHash("hello world");
            Assert.IsNotNull(hash);
            Assert.AreEqual("a67ee490ee7bff6898131cdc31e534338103c095", hash);
        }

        [TestMethod]
        public void VerifyFileReadAndWrite()
        {
            var testFile = "test-file-verifyfilereadandwrite.txt";
            QuranFileUtils.DeleteFile(testFile);

            Assert.AreEqual(string.Empty, QuranFileUtils.ReadFile(testFile));

            QuranFileUtils.WriteFile(testFile, "hello world");
            Assert.AreEqual("hello world", QuranFileUtils.ReadFile(testFile));
        }

        [TestMethod]
        public void DeleteDoesntThrowExceptionAndLogsFile()
        {
            var udPath = QuranFileUtils.GetUndeletedFilesDirectory(false);
            var testFile = "test-file-verifyfilereadandwrite.txt";
            QuranFileUtils.DeleteFile(testFile);
            QuranFileUtils.DeleteFolder(udPath);

            Assert.AreEqual(string.Empty, QuranFileUtils.ReadFile(testFile));

            QuranFileUtils.WriteFile(testFile, "hello world");
            using (var isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (var isoStream = new IsolatedStorageFileStream(testFile, FileMode.Open, isf))
                {
                    QuranFileUtils.DeleteFile(testFile);
                }
            }

            using (var isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                var files = isf.GetFileNames(udPath + "/*");
                Assert.IsTrue(files.Length > 0);
            }
        }

        [TestMethod]
        public void DeleteStuckFilesCleansFiles()
        {
            DeleteDoesntThrowExceptionAndLogsFile();
            var udPath = QuranFileUtils.GetUndeletedFilesDirectory(false);
            var testFile = "test-file-verifyfilereadandwrite.txt";

            QuranFileUtils.DeleteStuckFiles();

            using (var isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                var files = isf.GetFileNames(udPath + "/*");
                Assert.AreEqual(0, files.Length);
                Assert.IsFalse(isf.FileExists(testFile));
            }
        }

        [TestMethod]
        public void MakeDirectoryRecursivelyWorksWithNormalPath()
        {
            var path = "foo/bar/abc";
            QuranFileUtils.DeleteFolder(path);
            QuranFileUtils.MakeDirectoryRecursive(path);
            using (var isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                Assert.IsTrue(isf.DirectoryExists(path));
            }
        }

        [TestMethod]
        public void MakeDirectoryRecursivelyWorksWithInversePath()
        {
            var path = "foo\\bar\\abc";
            QuranFileUtils.DeleteFolder(path);
            QuranFileUtils.MakeDirectoryRecursive(path);
            using (var isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                Assert.IsTrue(isf.DirectoryExists(path));
            }
        }
    }
}
