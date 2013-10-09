using System.IO;
using QuranPhone.Data;
using QuranPhone.Utils;

namespace QuranPhone.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        public SettingsViewModel() { }

        #region Properties

        private string activeTranslation;
        private bool enableShowArabicInTranslation;
        private bool keepInfoOverlay;
        private bool nightMode;
        private bool preventPhoneFromSleeping;
        private bool showArabicInTranslation;

        private int textSize;

        public string ActiveTranslation
        {
            get { return activeTranslation; }
            set
            {
                if (value == activeTranslation)
                {
                    return;
                }

                activeTranslation = value;

                base.OnPropertyChanged(() => ActiveTranslation);
            }
        }

        public int TextSize
        {
            get { return textSize; }
            set
            {
                if (value == textSize)
                {
                    return;
                }

                textSize = value;
                SettingsUtils.Set(Constants.PrefTranslationTextSize, value);

                base.OnPropertyChanged(() => TextSize);
            }
        }

        public bool ShowArabicInTranslation
        {
            get { return showArabicInTranslation; }
            set
            {
                if (value == showArabicInTranslation)
                {
                    return;
                }

                showArabicInTranslation = value;
                SettingsUtils.Set(Constants.PrefShowArabicInTranslation, value);
                base.OnPropertyChanged(() => ShowArabicInTranslation);
            }
        }

        public bool EnableShowArabicInTranslation
        {
            get { return enableShowArabicInTranslation; }
            set
            {
                if (value == enableShowArabicInTranslation)
                {
                    return;
                }

                enableShowArabicInTranslation = value;

                base.OnPropertyChanged(() => EnableShowArabicInTranslation);
            }
        }

        public bool PreventPhoneFromSleeping
        {
            get { return preventPhoneFromSleeping; }
            set
            {
                if (value == preventPhoneFromSleeping)
                {
                    return;
                }

                preventPhoneFromSleeping = value;
                var oldValue = SettingsUtils.Get<bool>(Constants.PrefPreventSleep);
                SettingsUtils.Set(Constants.PrefPreventSleep, value);
                if (oldValue != value)
                {
                    PhoneUtils.ToggleIdleMode();
                }

                base.OnPropertyChanged(() => PreventPhoneFromSleeping);
            }
        }

        public bool NightMode
        {
            get { return nightMode; }
            set
            {
                if (value == nightMode)
                {
                    return;
                }

                nightMode = value;
                SettingsUtils.Set(Constants.PrefNightMode, value);
                base.OnPropertyChanged(() => NightMode);
            }
        }

        public bool KeepInfoOverlay
        {
            get { return keepInfoOverlay; }
            set
            {
                if (value == keepInfoOverlay)
                {
                    return;
                }

                keepInfoOverlay = value;
                SettingsUtils.Set(Constants.PrefKeepInfoOverlay, value);
                base.OnPropertyChanged(() => KeepInfoOverlay);
            }
        }

        #endregion Properties

        public void LoadData()
        {
            var translation = SettingsUtils.Get<string>(Constants.PrefActiveTranslation);
            if (!string.IsNullOrEmpty(translation) && translation.Contains("|"))
            {
                ActiveTranslation = translation.Split('|')[1];
            }
            else
            {
                ActiveTranslation = "?";
            }

            TextSize = SettingsUtils.Get<int>(Constants.PrefTranslationTextSize);
            ShowArabicInTranslation = SettingsUtils.Get<bool>(Constants.PrefShowArabicInTranslation);
            PreventPhoneFromSleeping = SettingsUtils.Get<bool>(Constants.PrefPreventSleep);
            KeepInfoOverlay = SettingsUtils.Get<bool>(Constants.PrefKeepInfoOverlay);
            NightMode = SettingsUtils.Get<bool>(Constants.PrefNightMode);

            if (
                QuranFileUtils.FileExists(Path.Combine(QuranFileUtils.GetQuranDatabaseDirectory(false),
                    QuranFileUtils.QuranArabicDatabase)))
            {
                EnableShowArabicInTranslation = true;
            }
            else
            {
                EnableShowArabicInTranslation = false;
            }
        }
    }
}