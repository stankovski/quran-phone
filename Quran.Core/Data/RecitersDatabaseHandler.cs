using System;
using System.Collections.Generic;
using System.IO;
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
                ServerUrl = "http://mirrors.quranicaudio.com/everyayah/Abdul_Basit_Murattal_192kbps/",
                IsGapless = false,
                GaplessDatabasePath = ""
            };
            inMemoryDatabase[3] = new ReciterItem
            {
                Name = "Abd Al-Basit Mujawwad",
                ServerUrl = "http://mirrors.quranicaudio.com/everyayah/Abdul_Basit_Mujawwad_128kbps/",
                IsGapless = false,
                GaplessDatabasePath = ""
            }; 
            inMemoryDatabase[4] = new ReciterItem
            {
                Name = "Abdullah Basfar",
                ServerUrl = "http://mirrors.quranicaudio.com/everyayah/Abdullah_Basfar_192kbps/",
                IsGapless = false,
                GaplessDatabasePath = ""
            };
            inMemoryDatabase[5] = new ReciterItem
            {
                Name = "Abdurrahmaan As-Sudais",
                ServerUrl = "http://mirrors.quranicaudio.com/everyayah/Abdurrahmaan_As-Sudais_192kbps/",
                IsGapless = false,
                GaplessDatabasePath = ""
            };
            inMemoryDatabase[6] = new ReciterItem
            {
                Name = "Abu Bakr Ash-Shatri",
                ServerUrl = "http://mirrors.quranicaudio.com/everyayah/Abu_Bakr_Ash-Shaatree_128kbps/",
                IsGapless = false,
                GaplessDatabasePath = ""
            };
            inMemoryDatabase[7] = new ReciterItem
            {
                Name = "Mishary Al-Afasy",
                ServerUrl = "http://mirrors.quranicaudio.com/everyayah/Alafasy_128kbps/",
                IsGapless = false,
                GaplessDatabasePath = ""
            };
            inMemoryDatabase[8] = new ReciterItem
            {
                Name = "Saad Al-Ghamdi",
                ServerUrl = "http://mirrors.quranicaudio.com/everyayah/Ghamadi_40kbps/",
                IsGapless = false,
                GaplessDatabasePath = ""
            };
            inMemoryDatabase[9] = new ReciterItem
            {
                Name = "Ibrahim Walk (English Trans.)",
                ServerUrl = "http://mirrors.quranicaudio.com/everyayah/Ibrahim_Walk_192kbps_TEST/",
                IsGapless = false,
                GaplessDatabasePath = ""
            };
            inMemoryDatabase[10] = new ReciterItem
            {
                Name = "Hani Ar-Rifai",
                ServerUrl = "http://mirrors.quranicaudio.com/everyayah/Hani_Rifai_192kbps/",
                IsGapless = false,
                GaplessDatabasePath = ""
            };
            inMemoryDatabase[11] = new ReciterItem
            {
                Name = "Husary Mujawwad",
                ServerUrl = "http://mirrors.quranicaudio.com/everyayah/Husary_128kbps_Mujawwad/",
                IsGapless = false,
                GaplessDatabasePath = ""
            };
            inMemoryDatabase[12] = new ReciterItem
            {
                Name = "Ali Al-Hudhaify",
                ServerUrl = "http://mirrors.quranicaudio.com/everyayah/Hudhaify_128kbps/",
                IsGapless = false,
                GaplessDatabasePath = ""
            };
            inMemoryDatabase[13] = new ReciterItem
            {
                Name = "Maher Al Muaiqly",
                ServerUrl = "http://mirrors.quranicaudio.com/everyayah/Maher_AlMuaiqly_64kbps/",
                IsGapless = false,
                GaplessDatabasePath = ""
            };
            inMemoryDatabase[14] = new ReciterItem
            {
                Name = "Minshawy Mujawwad",
                ServerUrl = "http://mirrors.quranicaudio.com/everyayah/Minshawy_Mujawwad_192kbps/",
                IsGapless = false,
                GaplessDatabasePath = ""
            };
            inMemoryDatabase[15] = new ReciterItem
            {
                Name = "Mohammad al Tablaway",
                ServerUrl = "http://mirrors.quranicaudio.com/everyayah/Mohammad_al_Tablaway_128kbps/",
                IsGapless = false,
                GaplessDatabasePath = ""
            };
            inMemoryDatabase[16] = new ReciterItem
            {
                Name = "Muhammad Ayyoub",
                ServerUrl = "http://mirrors.quranicaudio.com/everyayah/Muhammad_Ayyoub_128kbps/",
                IsGapless = false,
                GaplessDatabasePath = ""
            };
            inMemoryDatabase[17] = new ReciterItem
            {
                Name = "Muhammad Jibreel",
                ServerUrl = "http://mirrors.quranicaudio.com/everyayah/Muhammad_Jibreel_128kbps/",
                IsGapless = false,
                GaplessDatabasePath = ""
            };
            inMemoryDatabase[18] = new ReciterItem
            {
                Name = "Saood Ash-Shuraym",
                ServerUrl = "http://mirrors.quranicaudio.com/everyayah/Saood_ash-Shuraym_128kbps/",
                IsGapless = false,
                GaplessDatabasePath = ""
            };

            foreach (var id in inMemoryDatabase.Keys)
            {
                var reciter = inMemoryDatabase[id];
                reciter.Id = id;
                reciter.LocalPath = Path.Combine(FileUtils.GetQuranAudioDirectory().AsSync(),
                    reciter.Name.Replace(".", "").Replace("(", "").Replace(")", "").Replace(" ", "_"));
                if (reciter.IsGapless)
                    reciter.GaplessDatabasePath = Path.Combine(reciter.LocalPath, reciter.GaplessDatabasePath);
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
