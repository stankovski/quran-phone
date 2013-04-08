using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using QuranPhone.Data;

namespace Tests
{
    [TestClass]
    public class TestQuranInfo
    {
        [TestMethod]
        public void TestGetSurahName()
        {
            Assert.AreEqual("Surat Al-Baqara", QuranInfo.GetSuraName(2, true));
            Assert.AreEqual("Surat Al-Fatiha", QuranInfo.GetSuraName(1, true));
            Assert.AreEqual("Surat An-Nas", QuranInfo.GetSuraName(114, true));
            Assert.AreEqual("", QuranInfo.GetSuraName(0, true));
            Assert.AreEqual("", QuranInfo.GetSuraName(200, true));
        }
    }
}
