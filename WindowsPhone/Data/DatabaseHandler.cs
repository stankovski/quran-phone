using QuranPhone.Common;
using QuranPhone.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace QuranPhone.Data
{
    public class DatabaseHandler<T> : BaseDatabaseHandler where T: QuranAyah, new()
    {
        private string mDatabasePath = null;

        private int schemaVersion = 1;

        public DatabaseHandler(string databaseName)
        {
            string basePath = QuranFileUtils.GetQuranDatabaseDirectory(false, true);
            if (basePath == null) return;
            string path = basePath + QuranFileUtils.PATH_SEPARATOR + databaseName;
            mDatabase = new SQLiteDatabase(path);
            schemaVersion = GetSchemaVersion();
            mDatabasePath = path;
        }

        public bool ValidDatabase()
        {
            return (mDatabase == null) ? false : mDatabase.IsOpen();
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

        public int GetSchemaVersion()
        {
            int version = 1;
            if (!ValidDatabase()) { return version; }

            try
            {
                var result = mDatabase.Query<DatabaseProperties>().Where(p => p.Property == "schema_version").FirstOrDefault();
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
                var result = mDatabase.Query<DatabaseProperties>().Where(p => p.Property == "text_version").FirstOrDefault();
                if (result != null)
                    version = int.Parse(result.Value);
                return version;
            }
            catch (Exception)
            {
                return version;
            }
        }

        public List<T> GetVerses(int sura, int minAyah, int maxAyah)
        {
            return GetVerses(sura, minAyah, sura, maxAyah);
        }

        public virtual List<T> GetVerses(int minSura, int minAyah, int maxSura, int maxAyah)
        {
            if (!ValidDatabase())
            {
                if (!ReopenDatabase()) { return null; }
            }

            var result = mDatabase.Query<T>();

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

        public List<T> GetVerses(int page)
        {
            if (!ValidDatabase())
            {
                if (!ReopenDatabase()) { return null; }
            }

            int[] bound = QuranInfo.GetPageBounds(page);
            return GetVerses(bound[0], bound[1], bound[2], bound[3]);
        }

        public T GetVerse(int sura, int ayah)
        {
            return GetVerses(sura, ayah, ayah).FirstOrDefault();
        }

        public virtual List<T> Search(string query)
        {
            if (!ValidDatabase())
            {
                if (!ReopenDatabase()) { return null; }
            }

            // Get table name
            var tableName = "verses";
            var tableAttr = (TableAttribute)(typeof(T)).GetCustomAttributes(typeof(TableAttribute), true).FirstOrDefault();
            if (tableAttr != null)
                tableName = tableAttr.Name;

            // Couldn't get parameterized version to work - need to look into it in the future
            var sql = string.Format("select \"sura\", \"ayah\", \"text\" from \"{0}\" where \"text\" like '%{1}%' order by \"sura\", \"ayah\"", tableName, query);

            return mDatabase.Query<T>(sql).Take(50).ToList();          
        }        
    }
}
