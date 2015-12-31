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
        public const double ARABIC_FONT_SCALE_RELATIVE_TO_TRANSLATION = 1.5;

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
        public const string PREF_USE_ARABIC_RESHAPER = "useArabicReshaper";
        public const string PREF_LAST_PAGE = "lastPage";
        public const string PREF_LOCK_ORIENTATION = "lockOrientation";
        public const string PREF_LANDSCAPE_ORIENTATION = "landscapeOrientation";
        [DefaultValue(18.0)]
        public const string PREF_TRANSLATION_TEXT_SIZE = "translationTextSize";
        public const string PREF_ACTIVE_TRANSLATION = "activeTranslation";
        [DefaultValue(false)]
        public const string PREF_SHOW_TRANSLATION = "showTranslation";
        public const string PREF_RESHAPE_ARABIC = "reshapeArabic";
        [DefaultValue(false)]
        public const string PREF_SHOW_ARABIC_IN_TRANSLATION = "showArabicInTranslation";
        [DefaultValue(false)]
        public const string PREF_PREVENT_SLEEP = "preventSleep";
        [DefaultValue(true)]
        public const string PREF_KEEP_INFO_OVERLAY = "keepInfoOverlay";
        [DefaultValue(false)]
        public const string PREF_NIGHT_MODE = "nightMode";
        [DefaultValue(false)]
        public const string PREF_AUDIO_REPEAT = "audioRepeat";
        [DefaultValue(RepeatAmount.OneAyah)]
        public const string PREF_REPEAT_AMOUNT = "preferredRepeatAmount";
        [DefaultValue(0)]
        public const string PREF_REPEAT_TIMES = "preferredRepeatTimes";
        [DefaultValue("")]
        public const string PREF_ACTIVE_QARI = "activeQari";
        public const string PREF_SHOULD_FETCH_PAGES = "shouldFetchPages";
        public const string PREF_OVERLAY_PAGE_INFO = "overlayPageInfo";
        public const string PREF_DISPLAY_MARKER_POPUP = "displayMarkerPopup";
        public const string PREF_AYAH_BEFORE_TRANSLATION = "ayahBeforeTranslation";
        [DefaultValue(true)]
        public const string PREF_PREFER_STREAMING = "preferStreaming";
        [DefaultValue(false)]
        public const string PREF_ALT_DOWNLOAD = "altDownloadMethod";
        [DefaultValue(AudioDownloadAmount.Page)]
        public const string PREF_DOWNLOAD_AMOUNT = "preferredDownloadAmount";
        public const string PREF_LAST_UPDATED_TRANSLATIONS = "lastTranslationsUpdate";
        public const string PREF_HAVE_UPDATED_TRANSLATIONS = "haveUpdatedTranslations";
        public const string PREF_USE_NEW_BACKGROUND = "useNewBackground";
        public const string PREF_USE_VOLUME_KEY_NAV = "volumeKeyNavigation";
        [DefaultValue("0.0.0.0")]
        public const string PREF_CURRENT_VERSION = "currentVersion";
        [DefaultValue(false)]
        public const string SESSION_START_AUDIO = "_StartAudio";
    }
}
