using Quran.Core.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SQLite.Net;

namespace Quran.Core.Data
{
    public class TranslationsDatabaseHandler : BaseDatabaseHandler
    {
        public static string DbName = "translations.db";

        public TranslationsDatabaseHandler()
            : base(DbName)
        { }

        protected override SQLiteConnection CreateDatabase(string path)
        {
            var newDb = base.CreateDatabase(path);
            newDb.CreateTable<TranslationItem>();
            return newDb;
        }

        public List<TranslationItem> GetTranslations()
        {
            return dbConnection.Table<TranslationItem>().OrderBy(ti => ti.Id).ToList();
        }
    }
}
