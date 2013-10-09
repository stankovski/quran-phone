using QuranPhone.SQLite;

namespace QuranPhone.Common
{
    [Table("translations")]
    public class TranslationItem
    {
        public TranslationItem() {}

        public TranslationItem(string name)
        {
            Name = name;
            IsSeparator = false;
        }

        public TranslationItem(int id, string name, string translator, int latestVersion, string filename, string url,
            bool exists)
        {
            Id = id;
            Name = name;
            Translator = translator;
            Filename = filename;
            Url = url;
            Exists = exists;
            LatestVersion = latestVersion;
            IsSeparator = false;
        }

        [Column("id"), PrimaryKey]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("translator")]
        public string Translator { get; set; }

        [Column("filename")]
        public string Filename { get; set; }

        [Column("url")]
        public string Url { get; set; }

        [Ignore]
        public bool Compressed { get; set; }

        [Ignore]
        public bool Exists { get; set; }

        [Ignore]
        public int LatestVersion { get; set; }

        [Column("version")]
        public int LocalVersion { get; set; }

        [Ignore]
        public bool IsSeparator { get; set; }
    }
}