using FluentAssertions;
using Quran.Core.Common;
using Quran.Core.Utils;
using Xunit;

namespace Quran.Core.Tests.Utils
{
    public class QuranInfoTests : BaseTest
    {
        [Fact]
        public void GetSuraPagesWorksForGoodArguments()
        {
            Assert.Equal(7, QuranInfo.GetSuraNumberOfAyah(1));
            Assert.Equal(6, QuranInfo.GetSuraNumberOfAyah(114));
        }

        [Fact]
        public void GetSuraPagesWorksForBadArguments()
        {
            Assert.Equal(-1, QuranInfo.GetSuraNumberOfAyah(0));
            Assert.Equal(-1, QuranInfo.GetSuraNumberOfAyah(10000));
        }
    }
}
