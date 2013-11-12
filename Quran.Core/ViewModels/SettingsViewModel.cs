// --------------------------------------------------------------------------------------------------------------------
// <summary>
//    Defines the SettingsViewModel type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Quran.Core.Properties;
using Quran.Core.Utils;
using Quran.Core.Data;

namespace Quran.Core.ViewModels
{
    using System.Windows.Input;

    using Cirrious.MvvmCross.ViewModels;

    /// <summary>
    /// Define the SettingsViewModel type.
    /// </summary>
    public class SettingsViewModel : BaseViewModel
    {
        public SettingsViewModel()
        {
            SupportedLanguages = new ObservableCollection<KeyValuePair<string, string>>();
            foreach (var lang in GetSupportedLanguages())
            {
                SupportedLanguages.Add(lang);
            }
        }

        #region Properties
        private string activeTranslation;
        public string ActiveTranslation
        {
            get { return activeTranslation; }
            set
            {
                if (value == activeTranslation)
                    return;

                activeTranslation = value;

                base.RaisePropertyChanged(() => ActiveTranslation);
            }
        }

        private string activeReciter;
        public string ActiveReciter
        {
            get { return activeReciter; }
            set
            {
                if (value == activeReciter)
                    return;

                activeReciter = value;

                base.RaisePropertyChanged(() => ActiveReciter);
            }
        }

        private int textSize;
        public int TextSize
        {
            get { return textSize; }
            set
            {
                if (value == textSize)
                    return;

                textSize = value;
                SettingsUtils.Set(Constants.PREF_TRANSLATION_TEXT_SIZE, value);

                base.RaisePropertyChanged(() => TextSize);
            }
        }

        private bool showArabicInTranslation;
        public bool ShowArabicInTranslation
        {
            get { return showArabicInTranslation; }
            set
            {
                if (value == showArabicInTranslation)
                    return;

                showArabicInTranslation = value;
                SettingsUtils.Set(Constants.PREF_SHOW_ARABIC_IN_TRANSLATION, value);
                base.RaisePropertyChanged(() => ShowArabicInTranslation);
            }
        }

        private bool enableShowArabicInTranslation;
        public bool EnableShowArabicInTranslation
        {
            get { return enableShowArabicInTranslation; }
            set
            {
                if (value == enableShowArabicInTranslation)
                    return;

                enableShowArabicInTranslation = value;

                base.RaisePropertyChanged(() => EnableShowArabicInTranslation);
            }
        }

        private bool preventPhoneFromSleeping;
        public bool PreventPhoneFromSleeping
        {
            get { return preventPhoneFromSleeping; }
            set
            {
                if (value == preventPhoneFromSleeping)
                    return;

                preventPhoneFromSleeping = value;
                var oldValue = SettingsUtils.Get<bool>(Constants.PREF_PREVENT_SLEEP);
                SettingsUtils.Set(Constants.PREF_PREVENT_SLEEP, value);
                if (oldValue != value)
                {
                    QuranApp.NativeProvider.ToggleDeviceSleep(!value);
                }

                base.RaisePropertyChanged(() => PreventPhoneFromSleeping);
            }
        }

        private bool nightMode;
        public bool NightMode
        {
            get { return nightMode; }
            set
            {
                if (value == nightMode)
                    return;

                nightMode = value;

                // saving to setting utils
                SettingsUtils.Set(Constants.PREF_NIGHT_MODE, value);

                base.RaisePropertyChanged(() => NightMode);
            }
        }

        private bool keepInfoOverlay;
        public bool KeepInfoOverlay
        {
            get { return keepInfoOverlay; }
            set
            {
                if (value == keepInfoOverlay)
                    return;

                keepInfoOverlay = value;

                // saving to setting utils
                SettingsUtils.Set(Constants.PREF_KEEP_INFO_OVERLAY, value);

                base.RaisePropertyChanged(() => KeepInfoOverlay);
            }
        }

        private KeyValuePair<string, string> selectedLanguage;
        public KeyValuePair<string, string> SelectedLanguage
        {
            get { return selectedLanguage; }
            set
            {
                if (value.Key == selectedLanguage.Key)
                    return;

                selectedLanguage = value;

                if (SettingsUtils.Get<string>(Constants.PREF_CULTURE_OVERRIDE) != value.Key)
                {
                    QuranApp.NativeProvider.ShowInfoMessageBox(AppResources.please_restart);
                }

                // saving to setting utils
                SettingsUtils.Set(Constants.PREF_CULTURE_OVERRIDE, value.Key);

                base.RaisePropertyChanged(() => SelectedLanguage);
            }
        }

        public ObservableCollection<KeyValuePair<string, string>> SupportedLanguages { get; private set; }
        
        MvxCommand generate;
        /// <summary>
        /// Returns an download command
        /// </summary>
        public ICommand Generate
        {
            get
            {
                if (generate == null)
                {
                    generate = new MvxCommand(GenerateDua, () => this.CanGenerateDuaDownload);
                }
                return generate;
            }
        }

        public bool CanGenerateDuaDownload
        {
            get
            {
                return true;
            }
        }
        #endregion Properties

        #region Commands
        MvxCommand<string> navigateCommand;
        /// <summary>
        /// Returns a navigate command
        /// </summary>
        public ICommand NavigateCommand
        {
            get
            {
                if (navigateCommand == null)
                {
                    navigateCommand = new MvxCommand<string>(Navigate);
                }
                return navigateCommand;
            }
        }
        MvxCommand contactUsCommand;
        /// <summary>
        /// Returns a navigate command
        /// </summary>
        public ICommand ContactUsCommand
        {
            get
            {
                if (contactUsCommand == null)
                {
                    contactUsCommand = new MvxCommand(ContactUs);
                }
                return contactUsCommand;
            }
        }
        #endregion Commands

        public void LoadData()
        {
            var translation = SettingsUtils.Get<string>(Constants.PREF_ACTIVE_TRANSLATION);
            if (!string.IsNullOrEmpty(translation) && translation.Contains("|"))
                ActiveTranslation = translation.Split('|')[1];
            else
                ActiveTranslation = "None";

            var reciter = SettingsUtils.Get<string>(Constants.PREF_ACTIVE_QARI);
            if (!string.IsNullOrEmpty(reciter))
                ActiveReciter = reciter;
            else
                ActiveReciter = "None";

            SelectedLanguage = SupportedLanguages.FirstOrDefault(kv => kv.Key == SettingsUtils.Get<string>(Constants.PREF_CULTURE_OVERRIDE));
            TextSize = SettingsUtils.Get<int>(Constants.PREF_TRANSLATION_TEXT_SIZE);
            ShowArabicInTranslation = SettingsUtils.Get<bool>(Constants.PREF_SHOW_ARABIC_IN_TRANSLATION);
            PreventPhoneFromSleeping = SettingsUtils.Get<bool>(Constants.PREF_PREVENT_SLEEP);
            KeepInfoOverlay = SettingsUtils.Get<bool>(Constants.PREF_KEEP_INFO_OVERLAY);
            NightMode = SettingsUtils.Get<bool>(Constants.PREF_NIGHT_MODE);

            if (FileUtils.FileExists(PathHelper.Combine(FileUtils.GetQuranDatabaseDirectory(false),
                                                       FileUtils.QURAN_ARABIC_DATABASE)))
            {
                EnableShowArabicInTranslation = true;
            }
            else
            {
                EnableShowArabicInTranslation = false;
            }
        }

        public void GenerateDua()
        {
            DuaGenerator.Generate();
        }

        private void ContactUs()
        {
            QuranApp.NativeProvider.ComposeEmail("quran.phone@gmail.com", "Email from QuranPhone");
        }

        private void Navigate(string link)
        {
            if (link != null)
            {
                QuranApp.NativeProvider.LaunchWebBrowser(link);
            }
        }

        private IEnumerable<KeyValuePair<string, string>> GetSupportedLanguages()
        {
            yield return new KeyValuePair<string, string>("", "Default");
            var cultures = new string[] { "ar", "en", "id", "ru" };
            foreach (var c in cultures)
            {
                CultureInfo cultureInfo = null;
                try
                {
                    cultureInfo = new CultureInfo(c);
                }
                catch
                {
                    // Ignore
                }
                if (cultureInfo != null)
                {
                    yield return new KeyValuePair<string, string>(c, cultureInfo.NativeName);
                }
            }
        }
    }
}
