using Quran.Core.Common;

namespace Quran.Core.Utils
{
    public class RepeatInfo
    {
        private static long serialVersionUID = 1L;

        private int mRepeatCount;
        private int mCurrentAyah;
        private int mCurrentSura;
        private int mCurrentPlayCount;

        public RepeatInfo(int repeatCount)
        {
            mRepeatCount = repeatCount;
        }

        public void setCurrentVerse(int sura, int ayah)
        {
            if (sura != mCurrentSura || ayah != mCurrentAyah)
            {
                mCurrentSura = sura;
                mCurrentAyah = ayah;
                mCurrentPlayCount = 0;
            }
        }

        public int getRepeatCount()
        {
            return mRepeatCount;
        }

        public void setRepeatCount(int repeatCount)
        {
            mRepeatCount = repeatCount;
        }

        public bool shouldRepeat()
        {
            if (mRepeatCount == -1)
            {
                return true;
            }
            return (mCurrentPlayCount < mRepeatCount);
        }

        public void incrementRepeat()
        {
            mCurrentPlayCount++;
        }

        public QuranAyah getCurrentAyah()
        {
            return new QuranAyah(mCurrentSura, mCurrentAyah);
        }
    }
}
