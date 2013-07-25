using System.IO;
using System.IO.IsolatedStorage;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using QuranPhone.Common;
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
            string path = Path.Combine(basePath, QuranFileUtils.QURAN_ARABIC_DATABASE);

            var isf = IsolatedStorageFile.GetUserStoreForApplication();
            if (!isf.FileExists(path))
            {
                await QuranFileUtils.DownloadFileFromWebAsync(QuranFileUtils.GetArabicSearchDatabaseUrl(), path);
                return;
            }

            using (var dbh = new DatabaseHandler<QuranAyah>(QuranFileUtils.QURAN_ARABIC_DATABASE))
            {
                Assert.AreEqual(0, dbh.Search("test").Count);
            }
        }

        [TestMethod]
        public void TestGetVerseBounds()
        {
            string basePath = QuranFileUtils.GetQuranDatabaseDirectory(false);
            if (basePath == null) return;
            string path = Path.Combine(basePath, QuranFileUtils.GetAyaPositionFileName());

            var isf = IsolatedStorageFile.GetUserStoreForApplication();
            if (!isf.FileExists(path))
            {
                QuranFileUtils.DownloadFileFromWebAsync(QuranFileUtils.GetAyaPositionFileUrl(), path).Wait();
            }

            using (var dbh = new AyahInfoDatabaseHandler(QuranFileUtils.GetAyaPositionFileName()))
            {
                Assert.IsTrue(dbh.GetVerseBounds(2, 2).Count > 0);
            }
        }
    }
}
