using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using QuranPhone.Common;
using QuranPhone.SQLite;
using QuranPhone.Utils;

namespace QuranPhone.Data
{
    public class DatabaseHandler<T> : BaseDatabaseHandler where T : QuranAyah, new()
    {
        private readonly string _mDatabasePath;

        public DatabaseHandler(string databaseName)
        {
            string basePath = QuranFileUtils.GetQuranDatabaseDirectory(false, true);
            if (basePath == null)
            {
                return;
            }
            string path = Path.Combine(basePath, databaseName);
            MDatabase = new SQLiteDatabase(path);
            _mDatabasePath = path;
        }

        public bool ValidDatabase()
        {
            return (MDatabase != null) && MDatabase.IsOpen();
        }

        public bool ReopenDatabase()
        {
            try
            {
                MDatabase = new SQLiteDatabase(_mDatabasePath);
                return (MDatabase != null);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public int GetTextVersion()
        {
            int version = 1;
            if (!ValidDatabase())
            {
                return version;
            }

            try
            {
                DatabaseProperties result =
                    MDatabase.Query<DatabaseProperties>().FirstOrDefault(p => p.Property == "text_version");
                if (result != null)
                {
                    version = int.Parse(result.Value, CultureInfo.InvariantCulture);
                }
                return version;
            }
            catch (Exception)
            {
                return version;
            }
        }

        public IEnumerable<T> GetVerses(int sura, int minAyah, int maxAyah)
        {
            return GetVerses(sura, minAyah, sura, maxAyah);
        }

        public virtual List<T> GetVerses(int minSura, int minAyah, int maxSura, int maxAyah)
        {
            if (!ValidDatabase())
            {
                if (!ReopenDatabase())
                {
                    return null;
                }
            }

            TableQuery<T> result = MDatabase.Query<T>();

            if (minSura == maxSura)
            {
                result = result.Where(a => a.Sura == minSura && a.Ayah >= minAyah && a.Ayah <= maxAyah);
            }
            else
            {
                result =
                    result.Where(
                        a =>
                            (a.Sura == minSura && a.Ayah >= minAyah) || (a.Sura == maxSura && a.Ayah <= maxAyah) ||
                            (a.Sura > minSura && a.Sura < maxSura));
            }

            return result.OrderBy(a => a.Sura).OrderBy(a => a.Ayah).ToList();
        }

        public List<T> GetVerses(int page)
        {
            if (!ValidDatabase())
            {
                if (!ReopenDatabase())
                {
                    return null;
                }
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
                if (!ReopenDatabase())
                {
                    return null;
                }
            }

            // Get table name
            string tableName = "verses";

            // Couldn't get parameterized version to work - need to look into it in the future
            string sql =
                string.Format(
                    "select \"sura\", \"ayah\", \"text\" from \"{0}\" where \"text\" match '{1}' order by \"sura\", \"ayah\"",
                    tableName, query);

            return MDatabase.Query<T>(sql).Take(50).ToList();
        }
    }
}