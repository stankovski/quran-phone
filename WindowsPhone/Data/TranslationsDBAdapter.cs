using System.IO;
using QuranPhone.Common;
using QuranPhone.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuranPhone.Data
{
    public class TranslationsDBAdapter : BaseDatabaseHandler
    {
        public static string DB_NAME = "translations.db";

        public TranslationsDBAdapter()
        {
            string basePath = QuranFileUtils.GetQuranDatabaseDirectory(false, true);
            if (basePath == null) return;
            string path = Path.Combine(basePath, DB_NAME);

            if (!QuranFileUtils.FileExists(path))
                mDatabase = CreateDatabase(path);
            else
                mDatabase = new SQLiteDatabase(path);
        }

        private SQLiteDatabase CreateDatabase(string path)
        {
            var newDb = new SQLiteDatabase(path);
            newDb.CreateTable<TranslationItem>();
            return newDb;
        }

        public List<TranslationItem> GetTranslations()
        {
            return mDatabase.Query<TranslationItem>().OrderBy(ti => ti.Id).ToList();
        }

        public bool WriteTranslationUpdates(List<TranslationItem> updates)
        {
            bool result = true;
            mDatabase.BeginTransaction();
            try
            {
                foreach (var item in updates)
                {
                    var translation = mDatabase.Query<TranslationItem>().Where(ti => ti.Id == item.Id).FirstOrDefault();
                    if (item.Exists)
                    {
                        if (translation != null)
                        {
                            mDatabase.Update(item);
                        }
                        else
                        {
                            mDatabase.Insert(item);
                        }
                    }
                    else
                    {
                        mDatabase.Delete(item);
                    }
                }
                mDatabase.CommitTransaction();
            }
            catch (Exception e)
            {
                result = false;
                mDatabase.RollbackTransaction();
                Debug.WriteLine("error writing translation updates: " + e.Message);
            }

            return result;
        }
    }
}
