using SQLite;

namespace Cirrious.MvvmCross.Plugins.Sqlite.WindowsPhone
{
    public class MvxWindowsPhoneSQLiteConnectionFactory : ISQLiteConnectionFactory
    {
        public ISQLiteConnection Create(string address)
        {
            return new SQLiteConnection(address);
        }
    }
}
