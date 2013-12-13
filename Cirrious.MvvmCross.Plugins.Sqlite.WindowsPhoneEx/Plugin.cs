using Cirrious.CrossCore;
using Cirrious.CrossCore.Plugins;

namespace Cirrious.MvvmCross.Plugins.Sqlite.WindowsPhone
{
    public class Plugin
        : IMvxPlugin
    {
        public void Load()
        {
            Mvx.RegisterSingleton<ISQLiteConnectionFactory>(new MvxWindowsPhoneSQLiteConnectionFactory());
        }
    }
}
