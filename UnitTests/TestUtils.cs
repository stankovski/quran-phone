using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuranPhone.Utils;
using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UnitTests
{
    [TestClass]
    public class TestUtils
    {
        [TestMethod]
        public void TestMakeQuranDir()
        {
            // Setup
            IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication();
            if (isf.DirectoryExists(QuranFileUtils.GetQuranDirectory(false)))
                QuranFileUtils.DeleteFolder((QuranFileUtils.GetQuranDirectory(false)));
            Assert.IsFalse(isf.DirectoryExists(QuranFileUtils.GetQuranDirectory(false)));

            // Act
            Assert.IsTrue(QuranFileUtils.MakeQuranDirectory());

            // Verify
            Assert.IsTrue(isf.DirectoryExists(QuranFileUtils.GetQuranDirectory(false)));

            // Cleanup
            QuranFileUtils.DeleteFolder((QuranFileUtils.GetQuranDirectory(false)));
        }

        [TestMethod]
        public void TestMakeQuranDbDir()
        {
            // Setup
            IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication();
            if (isf.DirectoryExists(QuranFileUtils.GetQuranDatabaseDirectory(false)))
                QuranFileUtils.DeleteFolder((QuranFileUtils.GetQuranDatabaseDirectory(false)));
            Assert.IsFalse(isf.DirectoryExists(QuranFileUtils.GetQuranDatabaseDirectory(false)));

            // Act
            Assert.IsTrue(QuranFileUtils.MakeQuranDatabaseDirectory());

            // Verify
            Assert.IsTrue(isf.DirectoryExists(QuranFileUtils.GetQuranDatabaseDirectory(false)));

            // Cleanup
            QuranFileUtils.DeleteFolder((QuranFileUtils.GetQuranDatabaseDirectory(false)));
        }

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
    }
}
