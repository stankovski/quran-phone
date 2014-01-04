using System;
using FluentAssertions;
using Quran.Core.Common;
using Quran.Core.Utils;
using Xunit;
using Xunit.Extensions;

namespace Quran.Core.Tests.Utils
{
    public class QuranUtilsTests : BaseTest
    {
        [Fact]
        public void GetSuraPagesWorksForGoodArguments()
        {
            Assert.Equal(7, QuranUtils.GetSurahNumberOfAyah(1));
            Assert.Equal(6, QuranUtils.GetSurahNumberOfAyah(114));
        }

        [Fact]
        public void GetSuraPagesWorksForBadArguments()
        {
            Assert.Equal(-1, QuranUtils.GetSurahNumberOfAyah(0));
            Assert.Equal(-1, QuranUtils.GetSurahNumberOfAyah(10000));
        }

        [Theory]
        [InlineData(7, 1, 1, 1, 7)]
        [InlineData(8, 1, 1, 2, 1)]
        [InlineData(9, 1, 1, 2, 2)]
        [InlineData(1, 2, 2, 2, 2)]
        [InlineData(11, 114, 1, 1, 5)]
        public void GetAllAyahReturnsCorrectNumber(int expectedResult, int startSura, int startAya, int endSura, int endAya)
        {
            var list = QuranUtils.GetAllAyah(new QuranAyah(startSura, startAya), new QuranAyah(endSura, endAya));
            Assert.Equal(expectedResult, list.Count);
        }

        [Fact]
        public void QuranAyahFromStringAndToStringAreTheSame()
        {
            string ayahString = "1:2";
            var ayah = QuranAyah.FromString(ayahString);
            Assert.Equal(ayahString, ayah.ToString());
        }

        [Fact]
        public void QuranAyahFromStringThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => QuranAyah.FromString(null));
            Assert.Throws<ArgumentException>(() => QuranAyah.FromString(""));
            Assert.Throws<ArgumentException>(() => QuranAyah.FromString("1"));
            Assert.Throws<ArgumentException>(() => QuranAyah.FromString("a:b"));
        }

        [Theory]
        [InlineData(true, 1, 2)]
        [InlineData(true, 2, 0)]
        [InlineData(false, 0, 2)]
        [InlineData(false, 1, 12)]
        [InlineData(true, 1, 7)]
        [InlineData(true, 1, 1)]
        [InlineData(false, 1, 8)]
        public void IsValidWorks(bool expectedResult, int sura, int aya)
        {
            var suraAyah = new QuranAyah(sura, aya);
            Assert.Equal(expectedResult, QuranUtils.IsValid(suraAyah));
        }

        [Theory]
        [InlineData(new[] { 1, 1, 2, 25 }, 1)]
        [InlineData(new[] { 2, 26, 2, 43 }, 2)]
        [InlineData(new[] { 100, 9, 114, 6 }, 240)]
        public void GetRubBoundsWorks(int[] expectedResult, int rub)
        {
            var result = QuranUtils.GetRub3Bounds(rub);
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(new[] { 1, 1}, 1)]
        [InlineData(new[] { 2, 25 }, 1)]
        [InlineData(new[] { 2, 26 }, 2)]
        [InlineData(new[] { 100, 10 }, 240)]
        [InlineData(new[] { 114, 6 }, 240)]
        public void GetRubFromAyahWorks(int[] ayah, int expectedRub)
        {
            var result = QuranUtils.GetRub3FromAyah(ayah[0], ayah[1]);
            Assert.Equal(expectedRub, result);
        }

        [Theory]
        [InlineData(new[] { 1, 1, 2, 141 }, 1)]
        [InlineData(new[] { 2, 142, 2, 252 }, 2)]
        [InlineData(new[] { 78, 1, 114, 6 }, 30)]
        public void GetJuzBoundsWorks(int[] expectedResult, int juz)
        {
            var result = QuranUtils.GetJuzBounds(juz);
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(new[] { 1, 1, 2, 141 }, 1)]
        [InlineData(new[] { 2, 142, 2, 252 }, 2)]
        [InlineData(new[] { 78, 1, 114, 6 }, 30)]
        public void GetJuzFirstAyahWorks(int[] expectedResult, int juz)
        {
            var result = QuranUtils.GetJuzFirstAyah(juz);
            Assert.Equal(new QuranAyah(expectedResult[0], expectedResult[1]), result);
        }

        [Theory]
        [InlineData(new[] { 1, 1, 2, 141 }, 1)]
        [InlineData(new[] { 2, 142, 2, 252 }, 2)]
        [InlineData(new[] { 78, 1, 114, 6 }, 30)]
        public void GetJuzLastAyahWorks(int[] expectedResult, int juz)
        {
            var result = QuranUtils.GetJuzLastAyah(juz);
            Assert.Equal(new QuranAyah(expectedResult[2], expectedResult[3]), result);
        }
    }
}
