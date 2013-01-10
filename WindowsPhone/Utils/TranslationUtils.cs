using QuranPhone.Common;
using QuranPhone.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuranPhone.Utils
{
    public class TranslationUtils
    {
        public static String GetDefaultTranslation(IList<TranslationItem> items)
        {
            if (items == null || items.Count == 0) { return null; }

            string db = SettingsUtils.Get<string>(Constants.PREF_ACTIVE_TRANSLATION);

            bool changed = false;
            if (db == null)
            {
                changed = true;
                db = items[0].Filename;
            }
            else
            {
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
                }
            }

            if (changed && db != null)
            {
                SettingsUtils.Set<string>(Constants.PREF_ACTIVE_TRANSLATION, db);
            }

            return db;
        }
    }
}
