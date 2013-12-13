using Cirrious.CrossCore;
using Cirrious.MvvmCross.Plugins.Sqlite;
using Quran.Core.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Quran.Core.Data
{
    public class TranslationsDatabaseHandler : BaseDatabaseHandler
    {
        public static string DbName = "translations.db";

        public TranslationsDatabaseHandler()
            : base(DbName)
        { }

        protected override ISQLiteConnection CreateDatabase(ISQLiteConnectionFactory factory, string path)
        {
            var newDb = factory.Create(path);
            newDb.CreateTable<TranslationItem>();
            return newDb;
        }

        public List<TranslationItem> GetTranslations()
        {
            return dbConnection.Table<TranslationItem>().OrderBy(ti => ti.Id).ToList();
        }

        public bool WriteTranslationUpdates(List<TranslationItem> updates)
        {
            bool result = true;
            dbConnection.BeginTransaction();
            try
            {
                foreach (var item in updates)
                {
                    var translation = dbConnection.Table<TranslationItem>().FirstOrDefault(ti => ti.Id == item.Id);
                    if (item.Exists)
                    {
                        if (translation != null)
                        {
                            dbConnection.Update(item);
                        }
                        else
                        {
                            dbConnection.Insert(item);
                        }
                    }
                    else
                    {
                        dbConnection.Delete(item);
                    }
                }
                dbConnection.Commit();
            }
            catch (Exception e)
            {
                result = false;
                dbConnection.Rollback();
                Debug.WriteLine("error writing translation updates: " + e.Message);
            }

            return result;
        }
    }
}
