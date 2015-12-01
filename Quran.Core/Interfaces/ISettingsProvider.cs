namespace Quran.Core.Interfaces
{
    public interface ISettingsProvider
    {
        bool Contains(string key);
        void Save();
        object this[string key] {get;set;}
    }
}
