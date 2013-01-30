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

            IEnumerable<QuranAyah> result = mDatabase.Query<QuranAyah>();

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

        public virtual List<QuranAyah> Search(string query)
        {
            if (!ValidDatabase())
            {
                if (!ReopenDatabase()) { return null; }
            }

            return mDatabase.Query<QuranAyah>().Where(a => a.Text.Contains(query)).Take(50).ToList();            
        }        
    }

    public class ArabicDatabaseHandler : DatabaseHandler
    {
        public ArabicDatabaseHandler()
            : base("quran.ar.db")
        { }

        public override List<QuranAyah> GetVerses(int minSura, int minAyah, int maxSura,
                                int maxAyah, string table = "verses")
        {
            if (!ValidDatabase())
            {
                if (!ReopenDatabase()) { return null; }
            }

            StringBuilder sql = new StringBuilder("select \"sura\", \"ayah\", \"text\" from \"arabic_text\" where ");

            sql.Append("(");

            if (minSura == maxSura)
            {
                sql.Append("sura")
                        .Append("=").Append(minSura)
                        .Append(" and ").Append("ayah")
                        .Append(">=").Append(minAyah)
                        .Append(" and ").Append("ayah")
                        .Append("<=").Append(maxAyah);
            }
            else
            {
                // (sura = minSura and ayah >= minAyah)
                sql.Append("(").Append("sura").Append("=")
                        .Append(minSura).Append(" and ")
                        .Append("ayah").Append(">=").Append(minAyah).Append(")");

                sql.Append(" or ");

                // (sura = maxSura and ayah <= maxAyah)
                sql.Append("(").Append("sura").Append("=")
                        .Append(maxSura).Append(" and ")
                        .Append("ayah").Append("<=").Append(maxAyah).Append(")");

                sql.Append(" or ");

                // (sura > minSura and sura < maxSura)
                sql.Append("(").Append("sura").Append(">")
                        .Append(minSura).Append(" and ")
                        .Append("sura").Append("<")
                        .Append(maxSura).Append(")");
            }

            sql.Append(")");

            return mDatabase.Query<QuranAyah>(sql.ToString()).ToList();
        }

        public override List<QuranAyah> Search(string query)
        {
            if (!ValidDatabase())
            {
                if (!ReopenDatabase()) { return null; }
            }

            StringBuilder sql = new StringBuilder("select \"sura\", \"ayah\", \"text\" from \"arabic_text\" where \"text\" like '%?%'");

            return mDatabase.Query<QuranAyah>(sql.ToString(), query).Take(50).ToList();
        }  
    }
}
