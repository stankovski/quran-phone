using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuranPhone.Data;
using QuranPhone.Utils;
using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests
{
    [TestClass]
    public class TestBookmarks
    {
        [TestInitialize]
        public void TestCreateNewDatabase()
        {
            string basePath = QuranFileUtils.GetQuranDatabaseDirectory(false);
            if (basePath == null) return;
            string path = basePath + QuranFileUtils.PATH_SEPARATOR + BookmarksDBAdapter.DB_NAME;

            var isf = IsolatedStorageFile.GetUserStoreForApplication();
            if (isf.FileExists(path))
            {
                isf.DeleteFile(path);
            }

            using (var bookmarks = new BookmarksDBAdapter())
            {
                Assert.IsTrue(isf.FileExists(path));
            }
        }

        [TestMethod]
        public void TestCreateBookmark()
        {
            using (var bookmarks = new BookmarksDBAdapter())
            {
                var initialCount = bookmarks.GetBookmarks(false, BoomarkSortOrder.Alphabetical).Count;
                bookmarks.AddBookmark(5);
                var newCount = bookmarks.GetBookmarks(false, BoomarkSortOrder.Alphabetical).Count;
                Assert.AreEqual(initialCount + 1, newCount);
            }
        }

        [TestMethod]
        public void TestCreateTags()
        {
            using (var bookmarks = new BookmarksDBAdapter())
            {
                var initialCount = bookmarks.GetTags().Count;
                bookmarks.AddTag("test");
                var newCount = bookmarks.GetTags().Count;
                Assert.AreEqual(initialCount + 1, newCount);
            }
        }
    }
}
