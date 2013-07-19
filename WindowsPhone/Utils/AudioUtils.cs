using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuranPhone.Common;
using QuranPhone.Data;

namespace QuranPhone.Utils
{
    public enum LookAheadAmount
    {
        PAGE = 1,
        SURA = 2,
        JUZ = 3
    }

    public class AudioUtils {
       private const string TAG = "AudioUtils";
       public const string DB_EXTENSION = ".db";
       public const string AUDIO_EXTENSION = ".mp3";
       public const string ZIP_EXTENSION = ".zip";
       private const string AUDIO_DIRECTORY = "audio";
        public const int MIN = 1;
      public const int MAX = 3;


   private static string[] mQariBaseUrls = null;
   private static string[] mQariFilePaths = null;
   private static string[] mQariDatabaseFiles = null;

   public static string getQariUrl(int position,
                                   bool addPlaceHolders){
      if (mQariBaseUrls == null){
         mQariBaseUrls = context.getResources().getStringArray(R.array.quran_readers_urls);
      }

      if (position >= mQariBaseUrls.length || 0 > position){ return null; }
      string url = mQariBaseUrls[position];
      if (addPlaceHolders){
         if (isQariGapless(context, position)){
            Log.d(TAG, "qari is gapless...");
            url += "%03d" + AudioUtils.AUDIO_EXTENSION;
         }
         else { url += "%03d%03d" + AudioUtils.AUDIO_EXTENSION; }
      }
      return url;
   }

   public static string getLocalQariUrl(Context context, int position){
      if (mQariFilePaths == null){
         mQariFilePaths = context.getResources()
                 .getStringArray(R.array.quran_readers_path);
      }

      string rootDirectory = getAudioRootDirectory();
      return rootDirectory == null? null :
              rootDirectory + mQariFilePaths[position];
   }

   public static bool isQariGapless(Context context, int position){
      return getQariDatabasePathIfGapless(context, position) != null;
   }

   public static string getQariDatabasePathIfGapless(Context context,
                                                     int position){
      if (mQariDatabaseFiles == null){
         mQariDatabaseFiles = context.getResources()
                 .getStringArray(R.array.quran_readers_db_name);
      }

      if (position > mQariDatabaseFiles.length){ return null; }

      string dbname = mQariDatabaseFiles[position];
      Log.d(TAG, "got dbname of: " + dbname + " for qari");
      if (TextUtils.isEmpty(dbname)){ return null; }

      string path = getLocalQariUrl(context, position);
      if (path == null){ return null; }
      string overall = path + File.separator +
              dbname + DB_EXTENSION;
      Log.d(TAG, "overall path: " + overall);
      return overall;
   }

   public static bool shouldDownloadGaplessDatabase(
           Context context, DownloadAudioRequest request){
      if (!request.isGapless()){ return false; }
      string dbPath = request.getGaplessDatabaseFilePath();
      if (TextUtils.isEmpty(dbPath)){ return false; }

      File f = new File(dbPath);
      return !f.exists();
   }

   public static string getGaplessDatabaseUrl(
           Context context, DownloadAudioRequest request){
      if (!request.isGapless()){ return null; }
      int qariId = request.getQariId();

      if (mQariDatabaseFiles == null){
         mQariDatabaseFiles = context.getResources()
                 .getStringArray(R.array.quran_readers_db_name);
      }

      if (qariId > mQariDatabaseFiles.length){ return null; }

      string dbname = mQariDatabaseFiles[qariId] + ZIP_EXTENSION;
      return QuranFileUtils.GetGaplessDatabaseRootUrl() + "/" + dbname;
   }

   public static QuranAyah getLastAyahToPlay(QuranAyah startAyah,
                                             int page, int mode){
      int pageLastSura = 114;
      int pageLastAyah = 6;
      if (page > 604 || page < 0){ return null; }
      if (page < 604){
         int nextPageSura = QuranInfo.PAGE_SURA_START[page];
         int nextPageAyah = QuranInfo.PAGE_AYAH_START[page];

         pageLastSura = nextPageSura;
         pageLastAyah = nextPageAyah - 1;
         if (pageLastAyah < 1){
            pageLastSura--;
            if (pageLastSura < 1){ pageLastSura = 1; }
            pageLastAyah = QuranInfo.getNumAyahs(pageLastSura);
         }
      }

      if (mode == LookAheadAmount.SURA){
         int sura = startAyah.getSura();
         int lastAyah = QuranInfo.getNumAyahs(sura);
         if (lastAyah == -1){ return null; }

         // if we start playback between two suras, download both suras
         if (pageLastSura > sura){
            sura = pageLastSura;
            lastAyah = QuranInfo.getNumAyahs(sura);
         }
         return new QuranAyah(sura, lastAyah);
      }
      else if (mode == LookAheadAmount.JUZ){
         int juz = QuranInfo.getJuzFromPage(page);
         if (juz == 30){
            return new QuranAyah(114, 6);
         }
         else if (juz >= 1 && juz < 30){
            int[] endJuz = QuranInfo.QUARTERS[juz * 8];
            if (pageLastSura > endJuz[0]){
               // ex between jathiya and a7qaf
               endJuz = QuranInfo.QUARTERS[(juz+1) * 8];
            }
            else if (pageLastSura == endJuz[0] &&
                     pageLastAyah > endJuz[1]){
               // ex surat al anfal
               endJuz = QuranInfo.QUARTERS[(juz+1) * 8];
            }

            return new QuranAyah(endJuz[0], endJuz[1]);
         }
      }

      // page mode (fallback also from errors above)
      return new QuranAyah(pageLastSura, pageLastAyah);
   }

   public static bool shouldDownloadBasmallah(Context context,
                                                 DownloadAudioRequest request){
      if (request.isGapless()){ return false; }
      string baseDirectory = request.getLocalPath();
      if (!TextUtils.isEmpty(baseDirectory)){
         File f = new File(baseDirectory);
         if (f.exists()){
            string filename = 1 + File.separator + 1 + AUDIO_EXTENSION;
            f = new File(baseDirectory + File.separator + filename);
            if (f.exists()){
               android.util.Log.d(TAG, "already have basmalla...");
               return false; }
         }
         else {
            f.mkdirs();
         }
      }

      return doesRequireBasmallah(request);
   }

   public static bool haveSuraAyahForQari(string baseDir, int sura, int ayah){
      string filename = baseDir + File.separator + sura +
              File.separator + ayah + AUDIO_EXTENSION;
      File f = new File(filename);
      return f.exists();
   }

   private static bool doesRequireBasmallah(AudioRequest request){
      QuranAyah minAyah = request.getMinAyah();
      int startSura = minAyah.getSura();
      int startAyah = minAyah.getAyah();

      QuranAyah maxAyah = request.getMaxAyah();
      int endSura = maxAyah.getSura();
      int endAyah = maxAyah.getAyah();

      android.util.Log.d(TAG, "seeing if need basmalla...");

      for (int i = startSura; i <= endSura; i++){
         int lastAyah = QuranInfo.getNumAyahs(i);
         if (i == endSura){ lastAyah = endAyah; }
         int firstAyah = 1;
         if (i == startSura){ firstAyah = startAyah; }

         for (int j = firstAyah; j < lastAyah; j++){
            if (j == 1 && i != 1 && i != 9){
               android.util.Log.d(TAG, "need basmalla for " + i + ":" + j);

               return true;
            }
         }
      }

      return false;
   }

   public static bool haveAllFiles(DownloadAudioRequest request){
      string baseDirectory = request.getLocalPath();
      if (TextUtils.isEmpty(baseDirectory)){ return false; }

      bool isGapless = request.isGapless();
      File f = new File(baseDirectory);
      if (!f.exists()){
         f.mkdirs();
         return false;
      }

      QuranAyah minAyah = request.getMinAyah();
      int startSura = minAyah.getSura();
      int startAyah = minAyah.getAyah();

      QuranAyah maxAyah = request.getMaxAyah();
      int endSura = maxAyah.getSura();
      int endAyah = maxAyah.getAyah();

      for (int i = startSura; i <= endSura; i++){
         int lastAyah = QuranInfo.getNumAyahs(i);
         if (i == endSura){ lastAyah = endAyah; }
         int firstAyah = 1;
         if (i == startSura){ firstAyah = startAyah; }

         if (isGapless){
            if (i == endSura && endAyah == 0){ continue; }
            string p = request.getBaseUrl();
            string fileName = string.format(Locale.US, p, i);
            Log.d(TAG, "gapless, checking if we have " + fileName);
            f = new File(fileName);
            if (!f.exists()){ return false; }
            continue;
         }

         Log.d(TAG, "not gapless, checking each ayah...");
         for (int j = firstAyah; j < lastAyah; j++){
            string filename = i + File.separator + j + AUDIO_EXTENSION;
            f = new File(baseDirectory + File.separator + filename);
            if (!f.exists()){ return false; }
         }
      }

      return true;
   }

   public static string getAudioRootDirectory(){
      string s = QuranFileUtils.getQuranBaseDirectory();
      return (s == null)? null : s + AUDIO_DIRECTORY + File.separator;
   }

   public static string getOldAudioRootDirectory(Context context){
      File f = null;
      string path = "";
      string sep = File.separator;

      if (android.os.Build.VERSION.SDK_INT >= 8){
         f = context.getExternalFilesDir(null);
         path = sep + "audio" + sep;
      }
      else {
         f = Environment.getExternalStorageDirectory();
         path = sep + "Android" + sep + "data" + sep +
                 context.getPackageName() + sep + "files" + sep + "audio" + sep;
      }

      if (f == null){ return null; }
      return f.getAbsolutePath() + path;
   }
}
}
