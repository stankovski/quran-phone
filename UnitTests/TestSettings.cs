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
    public class TestSettings
    {
        [TestMethod]
        public void TestSetAndGet()
        {
            // Setup
            foreach (var k in IsolatedStorageSettings.ApplicationSettings.Keys)
            {
                if (k.ToString() == Constants.PREF_TRANSLATION_TEXT_SIZE)
                {
                    IsolatedStorageSettings.ApplicationSettings.Remove(k.ToString());
                    break;
                }
            }

            var val = SettingsUtils.Get<int>(Constants.PREF_TRANSLATION_TEXT_SIZE);
            Assert.AreEqual(0, val);
            SettingsUtils.Set<int>(Constants.PREF_TRANSLATION_TEXT_SIZE, 20);
            val = SettingsUtils.Get<int>(Constants.PREF_TRANSLATION_TEXT_SIZE);
            Assert.AreEqual(20, val);
        }
    }
}
