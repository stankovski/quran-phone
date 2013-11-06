using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cirrious.MvvmCross.Plugins.Sqlite;
using Quran.Core.Common;

namespace Quran.Core.Data
{
    public class SuraTimingDatabaseHandler : BaseDatabaseHandler
    {
        public SuraTimingDatabaseHandler(string databaseName) : base(databaseName)
        { }

        protected override ISQLiteConnection CreateDatabase(ISQLiteConnectionFactory factory, string path)
        {
            var newDb = factory.Create(path);
            newDb.CreateTable<Timings>();
            return newDb;
        }

        public List<Timings> GetAyahTimings(int sura)
        {
            return dbConnection.Table<Timings>().Where(t=>t.Sura == sura).ToList();
        }
    }
}
