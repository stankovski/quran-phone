namespace Quran.Core.Interfaces
{
    public interface ISettingsProvider
    {
        bool Contains(string key);
        void Save();
        void Add(string key, object value);
        object this[string key] {get;set;}
    }
}
