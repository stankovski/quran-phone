using QuranPhone.Common;
using QuranPhone.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuranPhone.Data
{
    public class DatabaseHandler
    {

        private SQLiteDatabase mDatabase = null;
        private string mDatabasePath = null;

        private int schemaVersion = 1;

        public DatabaseHandler(string databaseName)
        {
            string basePath = QuranFileUtils.GetQuranDatabaseDirectory(false);
            if (basePath == null) return;
            string path = basePath + QuranFileUtils.PATH_SEPARATOR + databaseName;
            mDatabase = new SQLiteDatabase(path);
            schemaVersion = GetSchemaVersion();
            mDatabasePath = path;
        }

        public bool ValidDatabase()
        {
            return (mDatabase == null) ? false : mDatabase.isOpen();
        }

        public bool ReopenDatabase()
        {
            try
            {
                mDatabase = new SQLiteDatabase(mDatabasePath);
                return (mDatabase != null);
            }
            catch (Exception) { return false; }
        }

        public List<QuranAyah> GetVerses(int sura, int minAyah, int maxAyah)
        {
            return GetVerses(sura, minAyah, maxAyah);
        }

        public int GetSchemaVersion()
        {
            int version = 1;
            if (!ValidDatabase()) { return version; }

            try
            {
                var result = mDatabase.query<DatabaseProperties>().Where(p => p.Property == "schema_version").FirstOrDefault();
                if (result != null)
                    version = int.Parse(result.Value);
                return version;
            }
            catch (Exception)
            {
                return version;
            }
        }

        public int GetTextVersion()
        {
            int version = 1;
            if (!ValidDatabase()) { return version; }

            try
            {
                var result = mDatabase.query<DatabaseProperties>().Where(p => p.Property == "text_version").FirstOrDefault();
                if (result != null)
                    version = int.Parse(result.Value);
                return version;
            }
            catch (Exception)
            {
                return version;
            }
        }

        public List<QuranAyah> GetVerses(int sura, int minAyah, int maxAyah, string table)
        {
            return GetVerses(sura, minAyah, sura, maxAyah, table);
        }

        public List<QuranAyah> GetVerses(int minSura, int minAyah, int maxSura,
                                int maxAyah, string table)
        {
            if (!ValidDatabase())
            {
                if (!ReopenDatabase()) { return null; }
            }

            IEnumerable<QuranAyah> result = mDatabase.query<QuranAyah>();

            if (minSura == maxSura)
            {
                result = result.Where(a => a.Sura == minSura && a.Ayah >= minAyah && a.Ayah <= maxAyah);
            }
            else
            {
                result = result.Where(a =>
                    (a.Sura == minSura && a.Ayah >= minAyah) ||
                    (a.Sura == maxSura && a.Ayah <= maxAyah) ||
                    (a.Sura > minSura && a.Sura < maxSura));
            }

            return result.ToList();
        }

        public List<QuranAyah> GetVerse(int sura, int ayah)
        {
            return GetVerses(sura, ayah, ayah);
        }

        public string Search(string query, bool withSnippets)
        {
            if (!ValidDatabase())
            {
                if (!ReopenDatabase()) { return null; }
            }

            string opr = " like '%";
            string endOperator = "%'";
            string whatTextToSelect = "text";
            string table = "verses";

            bool useFullTextIndex = (schemaVersion > 1);
            if (useFullTextIndex)
            {
                opr = " MATCH '";
                endOperator = "*'";
            }

            if (useFullTextIndex && withSnippets)
                whatTextToSelect = "snippet(" + table + ")";

            return "select " + "sura" + ", " + "ayah" +
                    ", " + whatTextToSelect + " from " + table + " where " + "text" + opr + query + endOperator + " limit 50";
        }

        public void CloseDatabase()
        {
            if (mDatabase != null)
            {
                mDatabase.close();
                mDatabase = null;
            }
        }
    }
}
