using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quran.Core.Utils;

namespace Quran.Core.Data
{
    public class Constants
    {
        // Numerics
        public const int DEFAULT_TEXT_SIZE = 15;
        public const double ARABIC_FONT_SCALE_RELATIVE_TO_TRANSLATION = 1.8;

        // 10 days in ms
        public const int TRANSLATION_REFRESH_TIME = 60 * 60 * 24 * 10 * 1000;

        // 1 hour in ms
        public const int MIN_TRANSLATION_REFRESH_TIME = 60 * 60 * 1000;

        // Pages
        public const int PAGES_FIRST = 1;
        public const int PAGES_LAST = 604;
        public const int SURA_TAWBA = 9;
        public const int SURA_FIRST = 1;
        public const int SURA_LAST = 114;
        public const int SURAS_COUNT = 114;
        public const int JUZ2_COUNT = 30;
        public const int AYA_MIN = 1;
        public const int AYA_MAX = 286;
        public const int NO_PAGE_SAVED = -1;

        // Settings Key
        [DefaultValue("")]
        public const string PREF_CULTURE_OVERRIDE = "cultureOverride";
        public const string PREF_LAST_PAGE = "lastPage";
        [DefaultValue(16.0)]
        public const string PREF_TRANSLATION_TEXT_SIZE = "translationTextSize";
        [DefaultValue(18.0)]
        public const string PREF_ARABIC_TEXT_SIZE = "arabicTextSize";
        public const string PREF_ACTIVE_TRANSLATION = "activeTranslation";
        [DefaultValue(false)]
        public const string PREF_SHOW_TRANSLATION = "showTranslation";
        [DefaultValue(false)]
        public const string PREF_SHOW_ARABIC_IN_TRANSLATION = "showArabicInTranslation";
        [DefaultValue(false)]
        public const string PREF_PREVENT_SLEEP = "preventSleep";
        [DefaultValue(true)]
        public const string PREF_KEEP_INFO_OVERLAY = "keepInfoOverlay";
        [DefaultValue(false)]
        public const string PREF_NIGHT_MODE = "nightMode";
        [DefaultValue("")]
        public const string PREF_ACTIVE_QARI = "activeQari";
        [DefaultValue(false)]
        public const string PREF_ALT_DOWNLOAD = "altDownloadMethod";
        public const string PREF_LAST_UPDATED_TRANSLATIONS = "lastTranslationsUpdate";
        [DefaultValue("0.0.0.0")]
        public const string PREF_CURRENT_VERSION = "currentVersion";

        // Temp settings
        [DefaultValue(-1)]
        public const string TEMP_PIVOT_STATE = "mainPivotState";
        public const string TEMP_SURAH_LIST_STATE = "mainSurahListState";
        public const string TEMP_JUZ_LIST_STATE = "mainJuzListState";
        public const string TEMP_BOOKMARKS_LIST_STATE = "mainBookmarksListState";
    }
}
