using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using QuranPhone.Common;
using QuranPhone.SQLite;
using QuranPhone.Utils;

namespace QuranPhone.Data
{
    public class TranslationsDBAdapter : BaseDatabaseHandler
    {
        public const string DbName = "translations.db";

        public TranslationsDBAdapter()
        {
            string basePath = QuranFileUtils.GetQuranDatabaseDirectory(false, true);
            if (basePath == null)
            {
                return;
            }
            string path = Path.Combine(basePath, DbName);

            if (!QuranFileUtils.FileExists(path))
            {
                MDatabase = CreateDatabase(path);
            }
            else
            {
                MDatabase = new SQLiteDatabase(path);
            }
        }

        private SQLiteDatabase CreateDatabase(string path)
        {
            var newDb = new SQLiteDatabase(path);
            newDb.CreateTable<TranslationItem>();
            return newDb;
        }

        public List<TranslationItem> GetTranslations()
        {
            return MDatabase.Query<TranslationItem>().OrderBy(ti => ti.Id).ToList();
        }

        public bool WriteTranslationUpdates(List<TranslationItem> updates)
        {
            bool result = true;
            MDatabase.BeginTransaction();
            try
            {
                foreach (TranslationItem item in updates)
                {
                    TranslationItem translation =
                        MDatabase.Query<TranslationItem>().Where(ti => ti.Id == item.Id).FirstOrDefault();
                    if (item.Exists)
                    {
                        if (translation != null)
                        {
                            MDatabase.Update(item);
                        }
                        else
                        {
                            MDatabase.Insert(item);
                        }
                    }
                    else
                    {
                        MDatabase.Delete(item);
                    }
                }
                MDatabase.CommitTransaction();
            }
            catch (Exception e)
            {
                result = false;
                MDatabase.RollbackTransaction();
                Debug.WriteLine("error writing translation updates: " + e.Message);
            }

            return result;
        }
    }
}