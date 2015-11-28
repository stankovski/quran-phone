// --------------------------------------------------------------------------------------------------------------------
// <summary>
//    Defines the SettingsViewModel type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Quran.Core.Data;
using Quran.Core.Properties;
using Quran.Core.Utils;

namespace Quran.Core.ViewModels
{
    

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
            SupportedAudioBlocks = new ObservableCollection<KeyValuePair<AudioDownloadAmount, string>>();
            foreach (var enumValue in Enum.GetNames(typeof(AudioDownloadAmount)))
            {
                SupportedAudioBlocks.Add(new KeyValuePair<AudioDownloadAmount, string>((AudioDownloadAmount)Enum.Parse(typeof(AudioDownloadAmount), enumValue), enumValue));
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

                base.OnPropertyChanged(() => ActiveTranslation);
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

                base.OnPropertyChanged(() => ActiveReciter);
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

                base.OnPropertyChanged(() => TextSize);
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
                base.OnPropertyChanged(() => ShowArabicInTranslation);
            }
        }

        private bool altDownloadMethod;
        public bool AltDownloadMethod
        {
            get { return altDownloadMethod; }
            set
            {
                if (value == altDownloadMethod)
                    return;

                altDownloadMethod = value;
                SettingsUtils.Set(Constants.PREF_ALT_DOWNLOAD, value);
                base.OnPropertyChanged(() => AltDownloadMethod);
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

                base.OnPropertyChanged(() => EnableShowArabicInTranslation);
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

                base.OnPropertyChanged(() => PreventPhoneFromSleeping);
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

                base.OnPropertyChanged(() => NightMode);
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

                base.OnPropertyChanged(() => KeepInfoOverlay);
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

                base.OnPropertyChanged(() => SelectedLanguage);
            }
        }

        private KeyValuePair<AudioDownloadAmount, string> selectedAudioBlock;
        public KeyValuePair<AudioDownloadAmount, string> SelectedAudioBlock
        {
            get { return selectedAudioBlock; }
            set
            {
                if (value.Key == selectedAudioBlock.Key)
                    return;

                selectedAudioBlock = value;

                SettingsUtils.Set(Constants.PREF_DOWNLOAD_AMOUNT, value.Key);

                base.OnPropertyChanged(() => SelectedAudioBlock);
            }
        }

        public ObservableCollection<KeyValuePair<string, string>> SupportedLanguages { get; private set; }

        public ObservableCollection<KeyValuePair<AudioDownloadAmount, string>> SupportedAudioBlocks { get; private set; }
        
        public bool CanGenerateDuaDownload
        {
            get
            {
                return true;
            }
        }
        #endregion Properties
        public override Task Initialize()
        {
            return Task.FromResult(0);
        }

        public async Task LoadData()
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
            SelectedAudioBlock = SupportedAudioBlocks.FirstOrDefault(kv => kv.Key == SettingsUtils.Get<AudioDownloadAmount>(Constants.PREF_DOWNLOAD_AMOUNT));
            TextSize = SettingsUtils.Get<int>(Constants.PREF_TRANSLATION_TEXT_SIZE);
            ShowArabicInTranslation = SettingsUtils.Get<bool>(Constants.PREF_SHOW_ARABIC_IN_TRANSLATION);
            AltDownloadMethod = SettingsUtils.Get<bool>(Constants.PREF_ALT_DOWNLOAD);
            PreventPhoneFromSleeping = SettingsUtils.Get<bool>(Constants.PREF_PREVENT_SLEEP);
            KeepInfoOverlay = SettingsUtils.Get<bool>(Constants.PREF_KEEP_INFO_OVERLAY);
            NightMode = SettingsUtils.Get<bool>(Constants.PREF_NIGHT_MODE);

            if (await FileUtils.FileExists(Path.Combine(await FileUtils.GetQuranDatabaseDirectory(),
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
