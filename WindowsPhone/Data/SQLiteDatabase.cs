using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Windows.Storage;

namespace QuranPhone.SQLite
{
    public class SQLiteDatabase : IDisposable
    {
        private SQLiteConnection dbConnection;

        public SQLiteDatabase(string inputFile)
        {
            dbConnection = new SQLiteConnection(Path.Combine(ApplicationData.Current.LocalFolder.Path, inputFile), false);
        }

        public void Dispose()
        {
            if (dbConnection != null)
            {
                dbConnection.Close();
                dbConnection = null;
            }
        }

        public bool IsOpen()
        {
            return dbConnection != null;
        }

        public IEnumerable<T> Query<T>(string query, params object[] args) where T : new()
        {
            return dbConnection.Query<T>(query, args);
        }

        public TableQuery<T> Query<T>() where T : new()
        {
            return dbConnection.Table<T>();
        }

        public int Insert(Object obj)
        {
            Type t = obj.GetType();
            PropertyInfo prop = t.GetProperty("AddedDate");
            if (prop != null)
            {
                // Change setValue to 3 parameters overload, to comply with QuranPhone7
                prop.SetValue(obj, DateTime.Now, null);
            }
            return dbConnection.Insert(obj);
        }

        public int Delete(Object obj)
        {
            return dbConnection.Delete(obj);
        }

        public int Update(Object obj)
        {
            return dbConnection.Update(obj);
        }

        public T ExecuteScalar<T>(string query, params object[] args)
        {
            return dbConnection.ExecuteScalar<T>(query, args);
        }

        public int Execute(string query, params object[] args)
        {
            return dbConnection.Execute(query, args);
        }

        public void BeginTransaction()
        {
            dbConnection.BeginTransaction();
        }

        public void CommitTransaction()
        {
            dbConnection.Commit();
        }

        public void RollbackTransaction()
        {
            dbConnection.Rollback();
        }

        public int CreateTable<T>()
        {
            return dbConnection.CreateTable<T>();
        }

        public void Close()
        {
            dbConnection.Close();
        }
    }
}