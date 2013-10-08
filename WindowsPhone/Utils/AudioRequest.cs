using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuranPhone.Common;
using QuranPhone.Data;

namespace QuranPhone.Utils
{
    public class AudioRequest {

   private static long serialVersionUID = 1L;
   private static String TAG = "AudioRequest";

   private String mBaseUrl = null;
   private String mGaplessDatabasePath = null;

   // where we started from
   private int mStartSura = 0;
   private int mStartAyah = 0;
   private int mAyahsInThisSura = 0;

   // min and max sura/ayah
   private int mMinSura = 0;
   private int mMinAyah = 0;
   private int mMaxSura = 0;
   private int mMaxAyah = 0;

   // what we're currently playing
   private int mCurrentSura = 0;
   private int mCurrentAyah = 0;

   // did we just play the basmallah?
   private bool mJustPlayedBasmallah = false;

   // repeat information
   private RepeatInfo mRepeatInfo;

   private int mQariId = -1;
   private String mLocalDirectoryPath = null;

   public AudioRequest(String baseUrl, QuranAyah verse){
      mBaseUrl = baseUrl;
      mStartSura = verse.Sura;
      mStartAyah = verse.Ayah;

      if (mStartSura < 1 || mStartSura > 114 || mStartAyah < 1){
         throw new ArgumentException();
      }
      
      mCurrentSura = mStartSura;
      mCurrentAyah = mStartAyah;
      mAyahsInThisSura = QuranInfo.SURA_NUM_AYAHS[mCurrentSura-1];

      mRepeatInfo = new RepeatInfo(0);
      mRepeatInfo.setCurrentVerse(mCurrentSura, mCurrentAyah);
   }

   public AudioRequest(String baseUrl, QuranAyah verse,
                               int qariId, String localPath) : this(baseUrl, verse) {
      mQariId = qariId;
      mLocalDirectoryPath = localPath;
   }

   public int getQariId(){ return mQariId; }
   public String getLocalPath(){ return mLocalDirectoryPath; }

   public void setGaplessDatabaseFilePath(String databaseFile){
      mGaplessDatabasePath = databaseFile;
   }

   public String getGaplessDatabaseFilePath(){
      return mGaplessDatabasePath;
   }

   public bool isGapless(){
      return mGaplessDatabasePath != null;
   }

   public void setRepeatInfo(RepeatInfo repeatInfo){
      if (repeatInfo == null){
         repeatInfo = new RepeatInfo(0);
      }
      if (mRepeatInfo != null){
         // just update the repeat count for now
         mRepeatInfo.setRepeatCount(repeatInfo.getRepeatCount());
      }
      else { mRepeatInfo = repeatInfo; }
   }

   public QuranAyah setCurrentAyah(int sura, int ayah){
      if (mRepeatInfo.shouldRepeat()){
         mRepeatInfo.incrementRepeat();
         return mRepeatInfo.getCurrentAyah();
      }
      else {
         mCurrentSura = sura;
         mCurrentAyah = ayah;
         mRepeatInfo.setCurrentVerse(mCurrentSura, mCurrentAyah);
         return null;
      }
   }

   public String getBaseUrl(){ return mBaseUrl; }

   public void setPlayBounds(QuranAyah minVerse, QuranAyah maxVerse){
      mMinSura = minVerse.Sura;
      mMinAyah = minVerse.Ayah;
      mMaxSura = maxVerse.Sura;
      mMaxAyah = maxVerse.Ayah;
   }

   public void removePlayBounds(){
      mMinSura = 0;
      mMinAyah = 0;
      mMaxSura = 0;
      mMaxAyah = 0;
   }

   public QuranAyah getMinAyah(){
      return new QuranAyah(mMinSura, mMinAyah);
   }

   public QuranAyah getMaxAyah(){
      return new QuranAyah(mMaxSura, mMaxAyah);
   }

   public String getUrl(){
      if ((mMaxSura > 0 && mCurrentSura > mMaxSura)
            || (mMaxAyah > 0 && mCurrentAyah > mMaxAyah
               && mCurrentSura >= mMaxSura)
            || (mMinSura > 0 && mCurrentSura < mMinSura)
            || (mMinAyah > 0 && mCurrentAyah < mMinAyah
               && mCurrentSura <= mMinSura)
            || mCurrentSura > 114
            || mCurrentSura < 1){
         return null;
      }

      if (isGapless()){
         String url = String.Format(mBaseUrl, mCurrentSura);
         return url;
      }

      int sura = mCurrentSura;
      int ayah = mCurrentAyah;
      if (ayah == 1 && sura != 1 && sura != 9 && !mJustPlayedBasmallah){
         mJustPlayedBasmallah = true;
         sura = 1;
         ayah = 1;
      }
      else { mJustPlayedBasmallah = false; }

      if (mJustPlayedBasmallah){
         // really if "about to play" bismillah...
         if (!haveSuraAyah(mCurrentSura, mCurrentAyah)){
            // if we don't have the first ayah, don't play basmallah
            return null;
         }
      }

      return String.Format(mBaseUrl, sura, ayah);
   }

   public bool haveSuraAyah(int sura, int ayah){
      // for streaming, we (theoretically) always "have" the sura and ayah
      return true;
   }
   
   public String getTitle(){
      return QuranInfo.GetSuraAyahString(mCurrentSura, mCurrentAyah);
   }

   public int getCurrentSura(){
      return mCurrentSura;
   }

   public int getCurrentAyah(){
      return mCurrentAyah;
   }
   
   public void gotoNextAyah(bool force){
      // don't go to next ayah if we haven't played basmallah yet
      if (mJustPlayedBasmallah){ return ; }
      if (!force && mRepeatInfo.shouldRepeat()){
         mRepeatInfo.incrementRepeat();
         if (mCurrentAyah == 1 && mCurrentSura != 1 && mCurrentSura != 9){
            mJustPlayedBasmallah = true;
         }
         return;
      }

      mCurrentAyah++;
      if (mAyahsInThisSura < mCurrentAyah){
         mCurrentAyah = 1;
         mCurrentSura++;
         if (mCurrentSura <= 114){
            mAyahsInThisSura = QuranInfo.SURA_NUM_AYAHS[mCurrentSura-1];
            mRepeatInfo.setCurrentVerse(mCurrentSura, mCurrentAyah);
         }
      }
      else { mRepeatInfo.setCurrentVerse(mCurrentSura, mCurrentAyah); }
   }

   public void gotoPreviousAyah(){
      mCurrentAyah--;
      if (mCurrentAyah < 1){
         mCurrentSura--;
         if (mCurrentSura > 0){
            mAyahsInThisSura = QuranInfo.SURA_NUM_AYAHS[mCurrentSura-1];
            mCurrentAyah = mAyahsInThisSura;
         }
      }
      else if (mCurrentAyah == 1 && !isGapless()){
         mJustPlayedBasmallah = true;
      }

      if (mCurrentSura > 0 && mCurrentAyah > 0){
         mRepeatInfo.setCurrentVerse(mCurrentSura, mCurrentAyah);
      }
   }
}
}
