using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quran.Core.Data;
using Xunit;

namespace Quran.Core.Tests.Database
{
    public class ReciterTests : BaseTest
    {
        [Fact]
        public void RecitersDatabaseHandlerContainsReciters()
        {
            var reciter = new RecitersDatabaseHandler();
            Assert.NotEmpty(reciter.GetAllReciters());
        }

        [Fact]
        public void GaplessRecitersContainDatabase()
        {
            var reciter = new RecitersDatabaseHandler();
            Assert.True(reciter.GetGaplessReciters().All(r => !string.IsNullOrWhiteSpace(r.GaplessDatabasePath)));
        }

        [Fact]
        public void NonGaplessRecitersDoNotContainDatabase()
        {
            var reciter = new RecitersDatabaseHandler();
            Assert.True(reciter.GetNonGaplessReciters().All(r => string.IsNullOrWhiteSpace(r.GaplessDatabasePath)));
        }

        [Fact]
        public void GaplessRecitersDatabaseEndWithDb()
        {
            var reciter = new RecitersDatabaseHandler();
            Assert.True(reciter.GetGaplessReciters().All(r => r.GaplessDatabasePath.EndsWith(".db")));
        }
    }
}
