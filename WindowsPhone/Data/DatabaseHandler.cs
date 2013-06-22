using QuranPhone.Common;
using QuranPhone.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuranPhone.Data
{
    public class DatabaseHandler : BaseDatabaseHandler
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

        public List<QuranAyah> GetVerses(int sura, int minAyah, int maxAyah, string table = "verses")
        {
            return GetVerses(sura, minAyah, sura, maxAyah, table);
        }

        public virtual List<QuranAyah> GetVerses(int minSura, int minAyah, int maxSura,
                                int maxAyah, string table = "verses")
        {
            if (!ValidDatabase())
            {
                if (!ReopenDatabase()) { return null; }
            }

            var result = mDatabase.Query<QuranAyah>();

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

        public List<QuranAyah> GetVerses(int page, string table = "verses")
        {
            if (!ValidDatabase())
            {
                if (!ReopenDatabase()) { return null; }
            }

            int[] bound = QuranInfo.GetPageBounds(page);
            return GetVerses(bound[0], bound[1], bound[2], bound[3], table);
        }

        public List<QuranAyah> GetVerse(int sura, int ayah)
        {
            return GetVerses(sura, ayah, ayah);
        }

        public virtual List<QuranAyah> Search(string query, string table = "verses")
        {
            if (!ValidDatabase())
            {
                if (!ReopenDatabase()) { return null; }
            }

            // Couldn't get parameterized version to work - need to look into it in the future
            var sql = string.Format("select \"sura\", \"ayah\", \"text\" from \"{0}\" where \"text\" like '%{1}%' order by \"sura\", \"ayah\"", table, query);

            return mDatabase.Query<QuranAyah>(sql).Take(50).ToList();          
        }        
    }
}
