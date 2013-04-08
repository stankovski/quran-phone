using System.IO.IsolatedStorage;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using QuranPhone.Data;
using QuranPhone.Utils;

namespace Tests
{
    [TestClass]
    public class TestDatabaseHandler
    {
        [TestMethod]
        public async void TestSql()
        {
            string basePath = QuranFileUtils.GetQuranDatabaseDirectory(false);
            if (basePath == null) return;
            string path = basePath + QuranFileUtils.PATH_SEPARATOR + QuranFileUtils.QURAN_ARABIC_DATABASE;

            var isf = IsolatedStorageFile.GetUserStoreForApplication();
            if (!isf.FileExists(path))
            {
                await QuranFileUtils.DownloadFileFromWebAsync(QuranFileUtils.GetArabicSearchDatabaseUrl(), path);
                return;
            }

            using (var dbh = new DatabaseHandler(QuranFileUtils.QURAN_ARABIC_DATABASE))
            {
                Assert.AreEqual(0, dbh.Search("test").Count);
            }
        }
    }
}
