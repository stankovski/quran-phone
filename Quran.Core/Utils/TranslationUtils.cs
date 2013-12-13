using Quran.Core.Common;
using Quran.Core.Data;
using System.Collections.Generic;

namespace Quran.Core.Utils
{
    public class TranslationUtils
    {
        public static string GetDefaultTranslation(IList<TranslationItem> items)
        {
            if (items == null || items.Count == 0) { return null; }

            string db = SettingsUtils.Get<string>(Constants.PREF_ACTIVE_TRANSLATION);
            string name;

            bool changed = false;
            if (db == null)
            {
                changed = true;
                db = items[0].Filename;
                name = items[0].Name;
            }
            else
            {
                db = db.Split('|')[0];
                name = db.Split('|')[1];
                bool found = false;
                foreach (TranslationItem item in items)
                {
                    if (item.Filename == db)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    changed = true;
                    db = items[0].Filename;
                    name = items[0].Name;
                }
            }

            if (changed && db != null)
            {
                SettingsUtils.Set<string>(Constants.PREF_ACTIVE_TRANSLATION, string.Join("|", db, name));
            }

            return db;
        }
    }
}
