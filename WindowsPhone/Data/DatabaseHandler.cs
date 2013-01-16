using QuranPhone.Common;
using QuranPhone.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuranPhone.Data
{
    public class DatabaseHandler {
	
	private SQLiteDatabase mDatabase = null;
    private string mDatabasePath = null;

    //public static string COL_SURA = "sura";
    //public static string COL_AYAH = "ayah";
    //public static string COL_TEXT = "text";
    //public static string VERSE_TABLE = "verses";
	public static string ARABIC_TEXT_TABLE = "arabic_text";
	public static string AR_SEARCH_TABLE = "search";
	public static string TRANSLITERATION_TABLE = "transliteration";
	
	public static string PROPERTIES_TABLE = "properties";
	public static string COL_PROPERTY = "property";
	public static string COL_VALUE = "value";
	
	private int schemaVersion = 1;
	
	public DatabaseHandler(string databaseName) 
    {
		string basePath = QuranFileUtils.GetQuranDatabaseDirectory(false);
		if (basePath == null) return;
		string path = basePath + QuranFileUtils.PATH_SEPARATOR + databaseName;
		mDatabase = new SQLiteDatabase(path);
		schemaVersion = getSchemaVersion();
        mDatabasePath = path;
	}
	
	public bool validDatabase(){
		return (mDatabase == null)? false : mDatabase.isOpen();
	}

   public bool reopenDatabase(){
      try {
         mDatabase = new SQLiteDatabase(mDatabasePath);
         return (mDatabase != null);
      }
      catch (Exception e){ return false; }
   }
	
	public List<QuranAyah> getVerses(int sura, int minAyah, int maxAyah){
		return getVerses(sura, minAyah, maxAyah);
	}
	
	public int getSchemaVersion(){
		int version = 1;
		if (!validDatabase()){ return version; }
		
		try {
			var result = mDatabase.query<DatabaseProperties>().Where(p=>p.Property == "schema_version").FirstOrDefault();
			if (result != null)
				version = int.Parse(result.Value);
			return version;
		}
		catch (Exception se){
			return version;
		}
	}

   public int getTextVersion(){
      int version = 1;
      if (!validDatabase()){ return version; }
		
		try {
			var result = mDatabase.query<DatabaseProperties>().Where(p=>p.Property == "text_version").FirstOrDefault();
			if (result != null)
				version = int.Parse(result.Value);
			return version;
		}
		catch (Exception se){
			return version;
		}
   }
	
	public List<QuranAyah> getVerses(int sura, int minAyah, int maxAyah, string table){
      return getVerses(sura, minAyah, sura, maxAyah, table);
   }

   public List<QuranAyah> getVerses(int minSura, int minAyah, int maxSura,
                           int maxAyah, string table){
      if (!validDatabase()){
         if (!reopenDatabase()){ return null; }
      }

      IEnumerable<QuranAyah> result = mDatabase.query<QuranAyah>();
       
      if (minSura == maxSura){
          result = result.Where(a => a.Sura == minSura && a.Ayah >= minAyah && a.Ayah <= maxAyah);
      }
      else {
          result = result.Where(a => 
              (a.Sura == minSura && a.Ayah >= minAyah) ||
              (a.Sura == maxSura && a.Ayah <= maxAyah) ||
              (a.Sura > minSura && a.Sura < maxSura));
      }

		return result.ToList();
	}
	
	public List<QuranAyah> getVerse(int sura, int ayah){
		return getVerses(sura, ayah, ayah);
	}
	
	public string search(string query, bool withSnippets){
		if (!validDatabase()){
         if (!reopenDatabase()){ return null; }
      }

		string opr = " like '%";
		string endOperator = "%'";
		string whatTextToSelect = "text";
        string table = "verses";
		
		bool useFullTextIndex = (schemaVersion > 1);
		if (useFullTextIndex){
			opr = " MATCH '";
			endOperator = "*'";
		}
		
		if (useFullTextIndex && withSnippets)
			whatTextToSelect = "snippet(" + table + ")";
		
		return "select " + "sura" + ", " + "ayah" +
				", " + whatTextToSelect + " from " + table + " where " + "text" + opr + query + endOperator + " limit 50";
	}
	
	public void closeDatabase() {
		if (mDatabase != null){
			mDatabase.close();
         mDatabase = null;
      }
	}
}
}
