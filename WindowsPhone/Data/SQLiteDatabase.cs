using SQLite;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
#if WINDOWS_PHONE_8
using Windows.Storage;
#else
using System.IO.IsolatedStorage;
#endif

public class SQLiteDatabase : IDisposable
{
    SQLiteConnection dbConnection;

    public SQLiteDatabase(string inputFile)
    {
#if WINDOWS_PHONE_8
        dbConnection = new SQLite.SQLiteConnection(Path.Combine(ApplicationData.Current.LocalFolder.Path, inputFile), false);
#else
        // Based on http://msdn.microsoft.com/en-us/library/windowsphone/develop/jj714087%28v=vs.105%29.aspx#BKMK_FilesandfoldersAPIoverview
        string dataFolderName = "";
        //IsolatedStorageFile localFile = IsolatedStorageFile.GetUserStoreForApplication();
        //if (!localFile.DirectoryExists(dataFolderName))
        //{
        //    localFile.CreateDirectory(dataFolderName);
        //}

        dbConnection = new SQLite.SQLiteConnection(Path.Combine(dataFolderName, inputFile), false);
#endif
    }

    public bool IsOpen()
    {
        return dbConnection != null;
    }

    public IEnumerable<T> Query<T>(string query, params object[] args) where T : new()
    {
        return dbConnection.Query<T>(query, args);
    }

    public IEnumerable<T> Query<T>() where T : new()
    {
        return dbConnection.Table<T>();
    }

    public int Insert(Object obj)
    {
        var t = obj.GetType();
        var prop = t.GetProperty("AddedDate");
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

    public void Dispose()
    {
        if (dbConnection != null)
        {
            dbConnection.Close();
            dbConnection = null;
        }
    }
}
