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
        { }

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

        private double textSize;
        public double TextSize
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

        private bool repeatAudio;
        public bool RepeatAudio
        {
            get { return repeatAudio; }
            set
            {
                if (value == repeatAudio)
                    return;

                repeatAudio = value;

                // saving to setting utils
                SettingsUtils.Set(Constants.PREF_AUDIO_REPEAT, value);

                base.OnPropertyChanged(() => RepeatAudio);
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

        private KeyValuePair<AudioDownloadAmount, string> selectedAudioBlock = new KeyValuePair<AudioDownloadAmount,string>((AudioDownloadAmount)100, "");
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

        private KeyValuePair<RepeatAmount, string> selectedRepeatAmount = new KeyValuePair<RepeatAmount,string>((RepeatAmount)100, "");
        public KeyValuePair<RepeatAmount, string> SelectedRepeatAmount
        {
            get { return selectedRepeatAmount; }
            set
            {
                if (value.Key == selectedRepeatAmount.Key)
                    return;

                selectedRepeatAmount = value;

                SettingsUtils.Set(Constants.PREF_REPEAT_AMOUNT, value.Key);

                base.OnPropertyChanged(() => SelectedRepeatAmount);
            }
        }

        private KeyValuePair<int, string> selectedRepeatTimes = new KeyValuePair<int,string>(-1, "");
        public KeyValuePair<int, string> SelectedRepeatTimes
        {
            get { return selectedRepeatTimes; }
            set
            {
                if (value.Key == selectedRepeatTimes.Key)
                    return;

                selectedRepeatTimes = value;

                SettingsUtils.Set(Constants.PREF_REPEAT_TIMES, value.Key);

                base.OnPropertyChanged(() => SelectedRepeatTimes);
            }
        }

        public ObservableCollection<KeyValuePair<string, string>> SupportedLanguages { get; private set; }

        public ObservableCollection<KeyValuePair<AudioDownloadAmount, string>> SupportedAudioBlocks { get; private set; }

        public ObservableCollection<KeyValuePair<RepeatAmount, string>> SupportedRepeatAmount { get; private set; }

        public ObservableCollection<KeyValuePair<int, string>> SupportedRepeatTimes { get; private set; }
        
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
            SupportedRepeatAmount = new ObservableCollection<KeyValuePair<RepeatAmount, string>>();
            foreach (var repeatValue in GetSupportedRepeatAmounts())
            {
                SupportedRepeatAmount.Add(new KeyValuePair<RepeatAmount, string>(repeatValue.Key, repeatValue.Value));
            }
            SupportedRepeatTimes = new ObservableCollection<KeyValuePair<int, string>>();
            foreach (var repeatValue in GetSupportedRepeatTimes())
            {
                SupportedRepeatTimes.Add(new KeyValuePair<int, string>(repeatValue.Key, repeatValue.Value));
            }

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

            SelectedLanguage = SupportedLanguages.First(kv => kv.Key == SettingsUtils.Get<string>(Constants.PREF_CULTURE_OVERRIDE));
            SelectedAudioBlock = SupportedAudioBlocks.First(kv => kv.Key == SettingsUtils.Get<AudioDownloadAmount>(Constants.PREF_DOWNLOAD_AMOUNT));
            SelectedRepeatAmount = SupportedRepeatAmount.First(kv => kv.Key == SettingsUtils.Get<RepeatAmount>(Constants.PREF_REPEAT_AMOUNT));
            SelectedRepeatTimes = SupportedRepeatTimes.First(kv => kv.Key == SettingsUtils.Get<int>(Constants.PREF_REPEAT_TIMES));
            RepeatAudio = SettingsUtils.Get<bool>(Constants.PREF_AUDIO_REPEAT);
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

        public void ContactUs()
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

        private IEnumerable<KeyValuePair<AudioDownloadAmount, string>> GetSupportedDownloadAmounts()
        {
            yield return new KeyValuePair<AudioDownloadAmount, string>(AudioDownloadAmount.Page, AppResources.quran_page);
            yield return new KeyValuePair<AudioDownloadAmount, string>(AudioDownloadAmount.Surah, AppResources.quran_sura_lower);
            yield return new KeyValuePair<AudioDownloadAmount, string>(AudioDownloadAmount.Juz, AppResources.quran_juz2_lower);
        }

        private IEnumerable<KeyValuePair<int, string>> GetSupportedRepeatTimes()
        {
            yield return new KeyValuePair<int, string>(0, AppResources.none);
            yield return new KeyValuePair<int, string>(1, "1");
            yield return new KeyValuePair<int, string>(2, "2");
            yield return new KeyValuePair<int, string>(3, "3");
            yield return new KeyValuePair<int, string>(5, "5");
            yield return new KeyValuePair<int, string>(10, "10");
            yield return new KeyValuePair<int, string>(int.MaxValue, AppResources.unlimited);
        }

        private IEnumerable<KeyValuePair<RepeatAmount, string>> GetSupportedRepeatAmounts()
        {
            yield return new KeyValuePair<RepeatAmount, string>(RepeatAmount.None, AppResources.none);
            yield return new KeyValuePair<RepeatAmount, string>(RepeatAmount.OneAyah, "1 " + QuranUtils.GetAyahTitle());
            yield return new KeyValuePair<RepeatAmount, string>(RepeatAmount.ThreeAyah, "3 " + QuranUtils.GetAyahTitle());
            yield return new KeyValuePair<RepeatAmount, string>(RepeatAmount.FiveAyah, "5 " + QuranUtils.GetAyahTitle());
            yield return new KeyValuePair<RepeatAmount, string>(RepeatAmount.TenAyah, "10 " + QuranUtils.GetAyahTitle());
            yield return new KeyValuePair<RepeatAmount, string>(RepeatAmount.Page, AppResources.quran_page);
            yield return new KeyValuePair<RepeatAmount, string>(RepeatAmount.Surah, AppResources.quran_sura_lower);
            yield return new KeyValuePair<RepeatAmount, string>(RepeatAmount.Rub, AppResources.quran_rub3);
            yield return new KeyValuePair<RepeatAmount, string>(RepeatAmount.Juz, AppResources.quran_juz2_lower);
        }
    }
}
