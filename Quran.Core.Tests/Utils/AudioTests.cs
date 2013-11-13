using System;
using System.Linq;
using FluentAssertions;
using Quran.Core.Common;
using Quran.Core.Data;
using Quran.Core.Utils;
using Xunit;
using Xunit.Extensions;

namespace Quran.Core.Tests.Utils
{
    public class AudioTests : BaseTest
    {
        [Fact]
        public void TestAudioRequestProperties()
        {
            var request = new AudioRequest(0, new QuranAyah(1, 2), LookAheadAmount.Page);
            Assert.Equal("Minshawi Murattal (gapless)", request.Reciter.Name);
            Assert.Equal(new QuranAyah(1, 2), request.CurrentAyah);
        }

        [Fact]
        public void AudioRequestThrowsArgumentNullException()
        {
            Assert.Throws(typeof(ArgumentNullException), () => new AudioRequest(null));
            Assert.Throws(typeof(ArgumentNullException), () => new AudioRequest(1, null, LookAheadAmount.Page));
            Assert.Throws(typeof(FormatException), () => new AudioRequest("Page/b/c/d"));
            Assert.Throws(typeof(ArgumentException), () => new AudioRequest("a/b/c/d"));
            Assert.Throws(typeof(ArgumentException), () => new AudioRequest("1/2"));
            Assert.Throws(typeof(ArgumentException), () => new AudioRequest(1, new QuranAyah(0, 0), LookAheadAmount.Page));
        }

        [Fact]
        public void AudioRequestWorksWithStringConstructor()
        {
            var request = new AudioRequest("Sura/0/1/2");
            Assert.Equal("Minshawi Murattal (gapless)", request.Reciter.Name);
            Assert.Equal(new QuranAyah(1, 2), request.CurrentAyah);
            Assert.Equal(LookAheadAmount.Sura, request.LookAheadAmount);
        }

        [Fact]
        public void AudioRequestToStringEqualsConstructor()
        {
            var pattern = "Sura/0/1/2";
            var request = new AudioRequest(pattern);
            Assert.Equal(pattern, request.ToString());
        }

        [Theory]
        [InlineData(1, 3, 1, 2)]
        [InlineData(2, 1, 1, 7)]
        [InlineData(114, 5, 114, 4)]
        [InlineData(1, 1, 114, 6)]
        public void AudioRequestGotoNextIncrementsAyah(int expSura, int expAya, int currSura, int currAya)
        {
            var request = new AudioRequest(0, new QuranAyah(currSura, currAya), LookAheadAmount.Page);
            request.GotoNextAyah();
            Assert.Equal(new QuranAyah(expSura, expAya), request.CurrentAyah);
        }

        [Theory]
        [InlineData(1, 1, 1, 2)]
        [InlineData(114, 6, 1, 1)]
        [InlineData(114, 3, 114, 4)]
        [InlineData(113, 5, 114, 1)]
        public void AudioRequestGotoPreviousDecrementsAyah(int expSura, int expAya, int currSura, int currAya)
        {
            var request = new AudioRequest(0, new QuranAyah(currSura, currAya), LookAheadAmount.Page);
            request.GotoPreviousAyah();
            Assert.Equal(new QuranAyah(expSura, expAya), request.CurrentAyah);
        }

        [Theory]
        [InlineData(true, 2, 1, 2, 2)]
        [InlineData(false, 2, 2, 2, 3)]
        [InlineData(false, 9, 1, 9, 5)]
        [InlineData(false, 8, 10, 9, 5)]
        [InlineData(true, 2, 200, 3, 5)]
        public void DoesRequireBasmallahWorks(bool result, int startSura, int startAya, int endSura, int endAya)
        {
            var requires = AudioUtils.DoesRequireBasmallah(new AudioRequest(5, new QuranAyah(startSura, startAya), LookAheadAmount.Page) { MaxAyah = new QuranAyah(endSura, endAya) });
            Assert.Equal(result, requires);
        }

        [Fact]
        public void DoesRequireBasmallahWorksWithoutMaxAyah()
        {
            var requires = AudioUtils.DoesRequireBasmallah(new AudioRequest(5, new QuranAyah(2, 1), LookAheadAmount.Page));
            Assert.True(requires);
        }

        [Fact]
        public void GetFileNameForGaplessAyahGetsCorrectName()
        {
            var database = new RecitersDatabaseHandler();
            var reciter = database.GetGaplessReciters().First(r => r.LocalPath.Contains("Minshawi_Murattal_gapless"));
            var fileName = AudioUtils.GetLocalPathForAyah(new QuranAyah(2, 1), reciter);
            Assert.Equal("quran_android/audio/Minshawi_Murattal_gapless/002.mp3", fileName.ToString());
        }

        [Fact]
        public void GetFileNameForNonGaplessAyahGetsCorrectName()
        {
            var database = new RecitersDatabaseHandler();
            var reciter = database.GetNonGaplessReciters().First(r => r.LocalPath.Contains("Abd_Al-Basit"));
            var fileName = AudioUtils.GetLocalPathForAyah(new QuranAyah(2, 1), reciter);
            Assert.Equal("quran_android/audio/Abd_Al-Basit/002001.mp3", fileName.ToString());
        }
    }
}
