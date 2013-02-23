using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuranPhone.Data
{
    public class Constants
    {
        // Numerics
        public const int DEFAULT_TEXT_SIZE = 15;
        public const double ARABIC_FONT_SCALE_RELATIVE_TO_TRANSLATION = 1.5;

        // 10 days in ms
        public const int TRANSLATION_REFRESH_TIME = 60 * 60 * 24 * 10 * 1000;

        // 1 hour in ms
        public const int MIN_TRANSLATION_REFRESH_TIME = 60 * 60 * 1000;

        // Pages
        public const int PAGES_FIRST = 1;
        public const int PAGES_LAST = 604;
        public const int SURA_FIRST = 1;
        public const int SURA_LAST = 114;
        public const int SURAS_COUNT = 114;
        public const int JUZ2_COUNT = 30;
        public const int AYA_MIN = 1;
        public const int AYA_MAX = 286;
        public const int NO_PAGE_SAVED = -1;

        // Settings Key
        public const String PREF_USE_ARABIC_NAMES = "useArabicNames";
        public const String PREF_USE_ARABIC_RESHAPER = "useArabicReshaper";
        public const String PREF_LAST_PAGE = "lastPage";
        public const String PREF_LOCK_ORIENTATION = "lockOrientation";
        public const String PREF_LANDSCAPE_ORIENTATION = "landscapeOrientation";
        [DefaultValue(25)]
        public const String PREF_TRANSLATION_TEXT_SIZE = "translationTextSize";
        [DefaultValue(35)]
        public const String PREF_ARABIC_TEXT_SIZE = "arabicTextSize";
        public const String PREF_ACTIVE_TRANSLATION = "activeTranslation";
        [DefaultValue(false)]
        public const String PREF_SHOW_TRANSLATION = "showTranslation";
        public const String PREF_RESHAPE_ARABIC = "reshapeArabic";
        [DefaultValue(false)]
        public const String PREF_SHOW_ARABIC_IN_TRANSLATION = "showArabicInTranslation";
        public const String PREF_NIGHT_MODE = "nightMode";
        public const String PREF_DEFAULT_QARI = "defaultQari";
        public const String PREF_SHOULD_FETCH_PAGES = "shouldFetchPages";
        public const String PREF_OVERLAY_PAGE_INFO = "overlayPageInfo";
        public const String PREF_DISPLAY_MARKER_POPUP = "displayMarkerPopup";
        public const String PREF_AYAH_BEFORE_TRANSLATION = "ayahBeforeTranslation";
        public const String PREF_PREFER_STREAMING = "preferStreaming";
        public const String PREF_DOWNLOAD_AMOUNT = "preferredDownloadAmount";
        public const String PREF_LAST_UPDATED_TRANSLATIONS = "lastTranslationsUpdate";
        public const String PREF_HAVE_UPDATED_TRANSLATIONS = "haveUpdatedTranslations";
        public const String PREF_USE_NEW_BACKGROUND = "useNewBackground";
        public const String PREF_USE_VOLUME_KEY_NAV = "volumeKeyNavigation";
    }
}
