using System;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Plugins.Sqlite;
using Quran.Core.Utils;

namespace Quran.Core.Data
{
    public abstract class BaseDatabaseHandler : IDisposable
    {
        protected ISQLiteConnection dbConnection = null;

        protected BaseDatabaseHandler(string databaseName)
        {
            var factory = Mvx.Resolve<ISQLiteConnectionFactory>();

            string basePath = QuranFileUtils.GetQuranDatabaseDirectory(false, true);
            if (basePath == null) return;
            string path = QuranFileUtils.Combine(QuranApp.NativeProvider.NativePath, basePath, databaseName);

            dbConnection = CreateDatabase(factory, path);
        }

        protected abstract ISQLiteConnection CreateDatabase(ISQLiteConnectionFactory factory, string path);

        public void Dispose()
        {
            if (dbConnection != null)
            {
                dbConnection.Close();
                dbConnection = null;
            }
        }
    }
}
