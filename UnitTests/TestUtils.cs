using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests
{
    [TestClass]
    public class TestUtils
    {
        [TestMethod]
        public void SimpleTest()
        {
            int a = 1;
            int b = 2;

            int c = a + b;

            Assert.IsTrue(c == 4);
        }
    }
}
