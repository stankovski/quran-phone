using System;
using System.Collections.Generic;
using System.Linq;
using Quran.Core.Common;
using Quran.Core.Utils;

namespace Quran.Core.Data
{
    public class RecitersDatabaseHandler
    {
        private Dictionary<int, ReciterItem> inMemoryDatabase = new Dictionary<int, ReciterItem>();

        public RecitersDatabaseHandler()
        {
            inMemoryDatabase[0] = new ReciterItem
            {
                Name = "Minshawi Murattal (gapless)",
                ServerUrl = "http://download.quranicaudio.com/quran/muhammad_siddeeq_al-minshaawee/",
                IsGapless = true,
                GaplessDatabasePath = "minshawi_murattal.db"
            };
            inMemoryDatabase[1] = new ReciterItem
            {
                Name = "Husary (gapless)",
                ServerUrl = "http://download.quranicaudio.com/quran/mahmood_khaleel_al-husaree/",
                IsGapless = true,
                GaplessDatabasePath = "minshawi_murattal.db"
            };
            inMemoryDatabase[2] = new ReciterItem
            {
                Name = "Abd Al-Basit",
                ServerUrl = "http://www.everyayah.com/data/Abdul_Basit_Murattal_64kbps/",
                IsGapless = false,
                GaplessDatabasePath = ""
            };
            inMemoryDatabase[3] = new ReciterItem
            {
                Name = "Abd Al-Basit Mujawwad",
                ServerUrl = "http://www.everyayah.com/data/Abdul_Basit_Mujawwad_128kbps/",
                IsGapless = false,
                GaplessDatabasePath = ""
            }; 
            inMemoryDatabase[4] = new ReciterItem
            {
                Name = "Abdullah Basfar",
                ServerUrl = "http://www.everyayah.com/data/Abdullah_Basfar_32kbps/",
                IsGapless = false,
                GaplessDatabasePath = ""
            };
            inMemoryDatabase[5] = new ReciterItem
            {
                Name = "Abdurrahmaan As-Sudais",
                ServerUrl = "http://www.everyayah.com/data/Abdurrahmaan_As-Sudais_64kbps/",
                IsGapless = false,
                GaplessDatabasePath = ""
            };
            inMemoryDatabase[6] = new ReciterItem
            {
                Name = "Abu Bakr Ash-Shatri",
                ServerUrl = "http://www.everyayah.com/data/Abu_Bakr_Ash-Shaatree_64kbps/",
                IsGapless = false,
                GaplessDatabasePath = ""
            };
            inMemoryDatabase[7] = new ReciterItem
            {
                Name = "Mishary Al-Afasy",
                ServerUrl = "http://www.everyayah.com/data/Alafasy_64kbps/",
                IsGapless = false,
                GaplessDatabasePath = ""
            };
            inMemoryDatabase[8] = new ReciterItem
            {
                Name = "Saad Al-Ghamdi",
                ServerUrl = "http://www.everyayah.com/data/Ghamadi_40kbps/",
                IsGapless = false,
                GaplessDatabasePath = ""
            };
            inMemoryDatabase[9] = new ReciterItem
            {
                Name = "Ibrahim Walk (English Trans.)",
                ServerUrl = "http://www.everyayah.com/data/English/Sahih_Intnl_Ibrahim_Walk_192kbps/",
                IsGapless = false,
                GaplessDatabasePath = ""
            };
            inMemoryDatabase[10] = new ReciterItem
            {
                Name = "Hani Ar-Rifai",
                ServerUrl = "http://www.everyayah.com/data/Hani_Rifai_64kbps/",
                IsGapless = false,
                GaplessDatabasePath = ""
            };
            inMemoryDatabase[11] = new ReciterItem
            {
                Name = "Husary Mujawwad",
                ServerUrl = "http://www.everyayah.com/data/Husary_Mujawwad_64kbps/",
                IsGapless = false,
                GaplessDatabasePath = ""
            };
            inMemoryDatabase[12] = new ReciterItem
            {
                Name = "Ali Al-Hudhaify",
                ServerUrl = "http://www.everyayah.com/data/Hudhaify_128kbps/",
                IsGapless = false,
                GaplessDatabasePath = ""
            };
            inMemoryDatabase[13] = new ReciterItem
            {
                Name = "Maher Al Muaiqly",
                ServerUrl = "http://www.everyayah.com/data/Maher_AlMuaiqly_64kbps/",
                IsGapless = false,
                GaplessDatabasePath = ""
            };
            inMemoryDatabase[14] = new ReciterItem
            {
                Name = "Nasser al Qatami",
                ServerUrl = "http://www.everyayah.com/data/Nasser_Alqatami_128kbps/",
                IsGapless = false,
                GaplessDatabasePath = ""
            };
            inMemoryDatabase[15] = new ReciterItem
            {
                Name = "Mohammad al Tablaway",
                ServerUrl = "http://www.everyayah.com/data/Mohammad_al_Tablaway_64kbps/",
                IsGapless = false,
                GaplessDatabasePath = ""
            };
            inMemoryDatabase[16] = new ReciterItem
            {
                Name = "Muhammad Ayyoub",
                ServerUrl = "http://www.everyayah.com/data/Muhammad_Ayyoub_128kbps/",
                IsGapless = false,
                GaplessDatabasePath = ""
            };
            inMemoryDatabase[17] = new ReciterItem
            {
                Name = "Muhammad Jibreel",
                ServerUrl = "http://www.everyayah.com/data/Muhammad_Jibreel_64kbps/",
                IsGapless = false,
                GaplessDatabasePath = ""
            };
            inMemoryDatabase[18] = new ReciterItem
            {
                Name = "Saood Ash-Shuraym",
                ServerUrl = "http://www.everyayah.com/data/Saood_ash-Shuraym_64kbps/",
                IsGapless = false,
                GaplessDatabasePath = ""
            };

            foreach (var id in inMemoryDatabase.Keys)
            {
                var reciter = inMemoryDatabase[id];
                reciter.Id = id;
                reciter.LocalPath = FileUtils.Combine(FileUtils.GetQuranAudioDirectory(false),
                    reciter.Name.Replace(".", "").Replace("(", "").Replace(")", "").Replace(" ", "_"));
                if (reciter.IsGapless)
                    reciter.GaplessDatabasePath = FileUtils.Combine(reciter.LocalPath, reciter.GaplessDatabasePath);
            }
        }

        public IEnumerable<ReciterItem> GetAllReciters()
        {
            return inMemoryDatabase.Values;
        }

        public IEnumerable<ReciterItem> GetGaplessReciters()
        {
            return inMemoryDatabase.Values.Where(r => r.IsGapless);
        }

        public IEnumerable<ReciterItem> GetNonGaplessReciters()
        {
            return inMemoryDatabase.Values.Where(r => !r.IsGapless);
        }

        public ReciterItem GetReciter(int id)
        {
            return inMemoryDatabase[id];
        }

        public ReciterItem GetReciter(string name)
        {
            return inMemoryDatabase.Values.FirstOrDefault(r => r.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }
    }
}
