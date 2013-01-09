using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuranPhone.Common
{
    public class TranslationItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Translator { get; set; }
        public string Filename { get; set; }
        public string Url { get; set; }
        public bool Exists { get; set; }
        public int LatestVersion { get; set; }
        public int LocalVersion { get; set; }
        public bool IsSeparator { get; set; }

        public TranslationItem(string name)
        {
            Name = name;
            IsSeparator = false;
        }

        public TranslationItem(int id, string name, string translator,
                               int latestVersion, string filename, string url,
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
    }
}
