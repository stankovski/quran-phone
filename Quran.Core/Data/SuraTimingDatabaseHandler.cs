using System.Collections.Generic;
using System.Linq;
using Quran.Core.Common;
using SQLite.Net;

namespace Quran.Core.Data
{
    public class SuraTimingDatabaseHandler : BaseDatabaseHandler
    {
        public SuraTimingDatabaseHandler(string databaseName) : base(databaseName)
        { }

        protected override SQLiteConnection CreateDatabase(string path)
        {
            var newDb = base.CreateDatabase(path);
            newDb.CreateTable<Timings>();
            return newDb;
        }

        public List<Timings> GetAyahTimings(int surah)
        {
            return dbConnection.Table<Timings>().Where(t=>t.Surah == surah).ToList();
        }
    }
}
