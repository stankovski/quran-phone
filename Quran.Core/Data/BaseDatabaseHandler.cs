using System;
using Quran.Core.Utils;
using SQLite.Net;
using SQLite.Net.Platform.WinRT;

namespace Quran.Core.Data
{
    public abstract class BaseDatabaseHandler : IDisposable
    {
        protected SQLiteConnection dbConnection = null;

        protected BaseDatabaseHandler(string databaseName)
        {
            string basePath = FileUtils.GetQuranDatabaseDirectory(false, true);
            if (basePath == null) return;
            string path = FileUtils.Combine(QuranApp.NativeProvider.NativePath, basePath, databaseName);

            dbConnection = CreateDatabase(path);
        }
       
        protected virtual SQLiteConnection CreateDatabase(string path)
        {
            return new SQLiteConnection(new SQLitePlatformWinRT(), path);
        }

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
