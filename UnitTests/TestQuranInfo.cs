using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuranPhone.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests
{
    [TestClass]
    public class TestQuranInfo
    {
        [TestMethod]
        public void TestGetSurahName()
        {
            Assert.AreEqual("Surat Al-Baqara", QuranInfo.getSuraName(2, true));
            Assert.AreEqual("Surat Al-Fatiha", QuranInfo.getSuraName(1, true));
            Assert.AreEqual("Surat An-Nas", QuranInfo.getSuraName(114, true));
            Assert.AreEqual("", QuranInfo.getSuraName(0, true));
            Assert.AreEqual("", QuranInfo.getSuraName(200, true));
        }
    }
}
