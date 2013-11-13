using FluentAssertions;
using Quran.Core.Common;
using Quran.Core.Utils;
using Xunit;
using Xunit.Extensions;

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

        [Theory]
        [InlineData(7, 1, 1, 1, 7)]
        [InlineData(8, 1, 1, 2, 1)]
        [InlineData(9, 1, 1, 2, 2)]
        [InlineData(1, 2, 2, 2, 2)]
        [InlineData(11, 114, 1, 1, 5)]
        public void GetAllAyahReturnsCorrectNumber(int expectedResult, int startSura, int startAya, int endSura, int endAya)
        {
            var list = QuranInfo.GetAllAyah(new QuranAyah(startSura, startAya), new QuranAyah(endSura, endAya));
            Assert.Equal(expectedResult, list.Count);
        }
    }
}
