using System;
using System.ComponentModel;

namespace QuranPhone.Data
{
    public class Constants
    {
        // Numerics
        public const int DefaultTextSize = 15;
        public const double ArabicFontScaleRelativeToTranslation = 1.5;
        public const int TranslationRefreshTime = 864000000;
        public const int MinimumTranslationRefreshTime = 3600000;

        // Pages
        public const int FirstPage = 1;
        public const int LastPage = 604;

        public const int FirstSura = 1;
        public const int LastSura = 114;

        public const int SuraCount = 114;
        public const int JuzConut = 30;

        public const int MinimumAya = 1;
        public const int MaximumAya = 286;

        // Settings Key
        public const String PrefLastPage = "lastPage";
        public const String PrefLockOrientation = "lockOrientation";
        public const String PrefLandscapeOrientation = "landscapeOrientation";

        [DefaultValue(25)]
        public const String PrefTranslationTextSize = "translationTextSize";
        public const String PrefActiveTranslation = "activeTranslation";

        [DefaultValue(false)]
        public const String PrefShowTranslation = "showTranslation";

        [DefaultValue(false)]
        public const String PrefShowArabicInTranslation = "showArabicInTranslation";

        [DefaultValue(false)]
        public const String PrefPreventSleep = "preventSleep";

        [DefaultValue(false)]
        public const String PrefKeepInfoOverlay = "keepInfoOverlay";

        [DefaultValue(false)]
        public const String PrefNightMode = "nightMode";

        public const String PrefDefaultQari = "defaultQari";
        public const String PrefShouldFetchPages = "shouldFetchPages";
        public const String PrefOverlayPageInfo = "overlayPageInfo";
        public const String PrefDisplayMarkerPopup = "displayMarkerPopup";
        public const String PrefAyahBeforeTranslation = "ayahBeforeTranslation";
        public const String PrefPreferStreaming = "preferStreaming";
        public const String PrefDownloadAmount = "preferredDownloadAmount";
        public const String PrefLastUpdatedTranslations = "lastTranslationsUpdate";
        public const String PrefHaveUpdatedTranslations = "haveUpdatedTranslations";
        public const String PrefUseNewBackground = "useNewBackground";
        public const String PrefUseVolumeKeyNav = "volumeKeyNavigation";
    }
}