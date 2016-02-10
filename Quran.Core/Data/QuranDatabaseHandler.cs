using System.Globalization;
using Quran.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using Quran.Core.Utils;
using Windows.Storage;

namespace Quran.Core.Data
{
    public class QuranDatabaseHandler<T> : BaseDatabaseHandler where T: QuranAyah, new()
    {
        public QuranDatabaseHandler(string databaseName)
            : base(databaseName)
        { }

        public QuranDatabaseHandler(StorageFile databaseFile)
            : base(databaseFile)
        { }

        public List<T> GetVerses(int surah, int minAyah, int maxAyah)
        {
            return GetVerses(surah, minAyah, surah, maxAyah);
        }

        public virtual List<T> GetVerses(int minSura, int minAyah, int maxSura, int maxAyah)
        {
            var result = dbConnection.Table<T>();

            if (minSura == maxSura)
            {
                result = result.Where(a => a.Surah == minSura && a.Ayah >= minAyah && a.Ayah <= maxAyah);
            }
            else
            {
                result = result.Where(a =>
                    (a.Surah == minSura && a.Ayah >= minAyah) ||
                    (a.Surah == maxSura && a.Ayah <= maxAyah) ||
                    (a.Surah > minSura && a.Surah < maxSura));
            }

            return result.OrderBy(a => a.Surah).OrderBy(a => a.Ayah).ToList();
        }

        public List<T> GetVerses(int page)
        {
            int[] bound = QuranUtils.GetPageBounds(page);
            return GetVerses(bound[0], bound[1], bound[2], bound[3]);
        }

        public T GetVerse(int surah, int ayah)
        {
            return GetVerses(surah, ayah, ayah).FirstOrDefault();
        }

        public virtual List<T> Search(string query)
        {
            // Get table name
            var tableName = "verses";

            // Couldn't get parameterized version to work - need to look into it in the future
            var sql = string.Format("select \"sura\", \"ayah\", \"text\" from \"{0}\" where \"text\" match '{1}' order by \"sura\", \"ayah\"", tableName, query);

            return dbConnection.Query<T>(sql).Take(50).ToList();          
        }        
    }
}
