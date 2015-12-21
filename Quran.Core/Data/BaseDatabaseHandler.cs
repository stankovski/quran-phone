using System;
using System.IO;
using Quran.Core.Utils;
using SQLite.Net;
using SQLite.Net.Platform.WinRT;

namespace Quran.Core.Data
{
    public abstract class BaseDatabaseHandler : IDisposable
    {
        protected SQLiteConnection dbConnection = null;

        protected BaseDatabaseHandler(string databaseName) : 
            this(FileUtils.GetQuranDatabaseDirectory(), databaseName)
        { }

        protected BaseDatabaseHandler(string basePath, string databaseName)
        {
            if (basePath == null)
            {
                throw new ArgumentNullException(nameof(basePath));
            }
            if (databaseName == null)
            {
                throw new ArgumentNullException(nameof(databaseName));
            }
            string path = Path.Combine(basePath, databaseName);

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
