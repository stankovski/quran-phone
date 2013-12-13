using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quran.Core.Data
{
    public class QuranDataProvider
    {
        //  public static string VERSES_MIME_TYPE = "vnd.android.cursor.dir/vnd.com.quran.labs.androidquran";
        //  public static string AYAH_MIME_TYPE = "vnd.android.cursor.item/vnd.com.quran.labs.androidquran";
        //  public static string QURAN_ARABIC_DATABASE = "quran.ar.db";

        // // UriMatcher stuff
        // private static int SEARCH_VERSES = 0;
        // private static int GET_VERSE = 1;
        // private static int SEARCH_SUGGEST = 2;
        // private static UriMatcher sURIMatcher = buildUriMatcher();

        //private string mCurrentLanguage = null;
        // private QuranDatabaseHandler dbConnection = null;

        // //private static UriMatcher buildUriMatcher() {
        // //    UriMatcher matcher =  new UriMatcher(UriMatcher.NO_MATCH);
        // //    matcher.addURI(AUTHORITY, "quran/search", SEARCH_VERSES);
        // //    matcher.addURI(AUTHORITY, "quran/search/*", SEARCH_VERSES);
        // //    matcher.addURI(AUTHORITY, "quran/search/*/*", SEARCH_VERSES);
        // //    matcher.addURI(AUTHORITY, "quran/verse/#/#", GET_VERSE);
        // //    matcher.addURI(AUTHORITY, "quran/verse/*/#/#", GET_VERSE);
        // //    matcher.addURI(AUTHORITY, SearchManager.SUGGEST_URI_PATH_QUERY,
        // //            SEARCH_SUGGEST);
        // //    matcher.addURI(AUTHORITY, SearchManager.SUGGEST_URI_PATH_QUERY + "/*", 
        // //            SEARCH_SUGGEST);
        // //    return matcher;
        // //}

        // public Cursor query(Uri uri, string[] projection, string selection,
        //         string[] selectionArgs, string sortOrder) {
        //     switch (sURIMatcher.match(uri)) {
        //     case SEARCH_SUGGEST:
        //         if (selectionArgs == null) {
        //             throw new IllegalArgumentException(
        //                     "selectionArgs must be provided for the Uri: " + uri);
        //         }

        //         return getSuggestions(selectionArgs[0]);
        //     case SEARCH_VERSES:
        //         if (selectionArgs == null) {
        //             throw new IllegalArgumentException(
        //                     "selectionArgs must be provided for the Uri: " + uri);
        //         }

        //         if (selectionArgs.length == 1)
        //             return search(selectionArgs[0]);
        //         else return search(selectionArgs[0], selectionArgs[1], true);
        //     case GET_VERSE:
        //         return getVerse(uri);
        //     default:
        //         throw new IllegalArgumentException("Unknown Uri: " + uri);
        //     }
        // }

        // private Cursor search(string query){
        //     if (QuranUtils.doesStringContainArabic(query) &&
        //             FileUtils.hasTranslation(QURAN_ARABIC_DATABASE)){
        //         Cursor c = search(query, QURAN_ARABIC_DATABASE, true);
        //         if (c != null) return c;
        //     }

        //     string active = getActiveTranslation();
        //     if (TextUtils.isEmpty(active)) return null;
        //     return search(query, active, true);
        // }

        // private string getActiveTranslation(){
        //     return mPrefs.getString(
        //           Constants.PREF_ACTIVE_TRANSLATION, "");
        // }

        // private Cursor getSuggestions(string query){
        //     if (query.length() < 3) return null;

        //     int numItems = 1;
        //     if (QuranUtils.doesStringContainArabic(query) &&
        //             FileUtils.hasTranslation(QURAN_ARABIC_DATABASE)){
        //         numItems = 2;
        //     }

        //     string[] items = new string[numItems];
        //     if (numItems == 1){ items[0] = getActiveTranslation(); }
        //     else {
        //         items[0] = QURAN_ARABIC_DATABASE;
        //         items[1] = getActiveTranslation();
        //     }

        //     string[] cols = new string[]{ BaseColumns._ID,
        //             SearchManager.SUGGEST_COLUMN_TEXT_1,
        //             SearchManager.SUGGEST_COLUMN_TEXT_2,
        //             SearchManager.SUGGEST_COLUMN_INTENT_DATA_ID };
        //     MatrixCursor mc = new MatrixCursor(cols);

        //   Context context = getContext();
        //     bool gotResults = false;
        //     for (string item : items) {
        //         if (gotResults){ continue; }
        //         Cursor suggestions = search(query, item, false);

        //         if (suggestions.moveToFirst()){
        //             do {
        //                 int surah = suggestions.getInt(0);
        //                 int ayah = suggestions.getInt(1);
        //                 string text = suggestions.getString(2);
        //                 string foundText = context
        //                    .getString(R.string.found_in_sura) +
        //                    " " + QuranUtils.getSuraName(context, surah, false) +
        //                    ", " + context.getString(R.string.quran_ayah) +
        //                    " " + ayah;

        //                 gotResults = true;
        //                 MatrixCursor.RowBuilder row = mc.newRow();
        //                 int id = 0;
        //                 for (int j=1; j<surah;j++){
        //                     id += QuranUtils.getNumAyahs(j);
        //                 }
        //                 id += ayah;

        //                 row.add(id);
        //                 row.add(text);
        //                 row.add(foundText);
        //                 row.add(id);
        //             } while (suggestions.moveToNext());
        //         }
        //         suggestions.close();
        //     }

        //     return mc;
        // }

        // private Cursor search(string query, string language, bool wantSnippets) {
        //     Log.d("qdp", "q: " + query + ", l: " + language);
        //     if (language == null) return null;

        //     if (dbConnection == null){
        //         dbConnection = new QuranDatabaseHandler(language);
        //   }
        //   else if (language != null && !language.equals(mCurrentLanguage)){
        //      dbConnection.closeDatabase();
        //      dbConnection = new QuranDatabaseHandler(language);
        //      mCurrentLanguage = language;
        //   }

        //     return dbConnection.search(query, wantSnippets);
        // }

        // private Cursor getVerse(Uri uri){
        //     int surah = 1;
        //     int ayah = 1;
        //     string langType = getActiveTranslation();
        //     string lang = (TextUtils.isEmpty(langType))? null : langType;
        //     if (lang == null) return null;

        //     List<string> parts = uri.getPathSegments();
        //     for (string s : parts)
        //         Log.d("qdp", "uri part: " + s);

        //     if (dbConnection == null){
        //         dbConnection = new QuranDatabaseHandler(lang);
        //   }
        //   else if (lang != null && !lang.equals(mCurrentLanguage)){
        //      dbConnection.closeDatabase();
        //      dbConnection = new QuranDatabaseHandler(lang);
        //      mCurrentLanguage = lang;
        //   }

        //     return dbConnection.getVerse(surah, ayah);
        // }
    }
}
