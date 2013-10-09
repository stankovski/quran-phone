using System;
using QuranPhone.SQLite;

namespace QuranPhone.Data
{
    public class BaseDatabaseHandler : IDisposable
    {
        protected SQLiteDatabase MDatabase;

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            CloseDatabase();
        }

        public void CloseDatabase()
        {
            if (MDatabase != null)
            {
                MDatabase.Close();
                MDatabase = null;
            }
        }
    }
}