using SQLite;
using System;
using System.Collections.Generic;
using System.Data;

class SQLiteDatabase
{
    SQLiteConnection dbConnection;

    public SQLiteDatabase(string inputFile)
    {
        dbConnection = new SQLite.SQLiteConnection(inputFile);
    }

    public bool isOpen()
    {
        return dbConnection != null;
    }

    public IList<T> query<T>()
    {
        var command = new SQLiteCommand(dbConnection);
        return command.ExecuteQuery<T>();
    }

    internal void close()
    {
        dbConnection.Close();
    }
}
