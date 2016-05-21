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
using Microsoft.ApplicationInsights;
using Quran.Core.Data;
using Quran.Core.Properties;
using Quran.Core.Utils;
using Windows.ApplicationModel.Core;

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

                base.OnPropertyChanged(() => TextSize);
            }
        }

        private double arabicTextSize;
        public double ArabicTextSize
        {
            get { return arabicTextSize; }
            set
            {
                if (value == arabicTextSize)
                    return;

                arabicTextSize = value;

                base.OnPropertyChanged(() => ArabicTextSize);
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
                if (oldValue != value)
                {
                    QuranApp.NativeProvider.ToggleDeviceSleep(!value);
                    SettingsUtils.Set(Constants.PREF_PREVENT_SLEEP, value);
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

                SettingsUtils.Set(Constants.PREF_KEEP_INFO_OVERLAY, value);

                base.OnPropertyChanged(() => KeepInfoOverlay);
            }
        }

        private string selectedLanguage;
        public string SelectedLanguage
        {
            get { return selectedLanguage; }
            set
            {
                if (value == selectedLanguage)
                    return;

                selectedLanguage = value;

                if (SettingsUtils.Get<string>(Constants.PREF_CULTURE_OVERRIDE) != value)
                {
                    SettingsUtils.Set(Constants.PREF_CULTURE_OVERRIDE, value);
                    PromptForShutdown();
                }

                base.OnPropertyChanged(() => SelectedLanguage);
            }
        }

        public async Task PromptForShutdown()
        {
            if (await QuranApp.NativeProvider.ShowQuestionMessageBox(Resources.please_restart))
            {
                CoreApplication.Exit();
            }
        }

        public ObservableCollection<KeyValuePair<string, string>> SupportedLanguages { get; private set; }

        public ObservableCollection<KeyValuePair<string, string>> SupportedAudioBlocks { get; private set; }

        #endregion Properties
        public override Task Initialize()
        {
            return Refresh();
        }

        public override async Task Refresh()
        {
            SupportedLanguages = new ObservableCollection<KeyValuePair<string, string>>();
            foreach (var lang in GetSupportedLanguages())
            {
                SupportedLanguages.Add(lang);
            }
            SupportedAudioBlocks = new ObservableCollection<KeyValuePair<string, string>>();
            foreach (var enumValue in Enum.GetNames(typeof(AudioDownloadAmount)))
            {
                SupportedAudioBlocks.Add(new KeyValuePair<string, string>(enumValue, enumValue));
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

            SelectedLanguage = SettingsUtils.Get<string>(Constants.PREF_CULTURE_OVERRIDE);
            TextSize = SettingsUtils.Get<double>(Constants.PREF_TRANSLATION_TEXT_SIZE);
            ArabicTextSize = SettingsUtils.Get<double>(Constants.PREF_ARABIC_TEXT_SIZE);
            ShowArabicInTranslation = SettingsUtils.Get<bool>(Constants.PREF_SHOW_ARABIC_IN_TRANSLATION);
            AltDownloadMethod = SettingsUtils.Get<bool>(Constants.PREF_ALT_DOWNLOAD);
            PreventPhoneFromSleeping = SettingsUtils.Get<bool>(Constants.PREF_PREVENT_SLEEP);
            KeepInfoOverlay = SettingsUtils.Get<bool>(Constants.PREF_KEEP_INFO_OVERLAY);
            NightMode = SettingsUtils.Get<bool>(Constants.PREF_NIGHT_MODE);
        }

        public void SaveSettings()
        {
            SettingsUtils.Set(Constants.PREF_TRANSLATION_TEXT_SIZE, TextSize);
            SettingsUtils.Set(Constants.PREF_ARABIC_TEXT_SIZE, ArabicTextSize);
        }


        public void GenerateDua()
        {
            telemetry.TrackEvent("GenerateDua");
            DuaGenerator.Generate();
        }

        public void ContactUs()
        {
            QuranApp.NativeProvider.LaunchWebBrowser("https://github.com/stankovski/quran-phone/issues");
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

        private IEnumerable<KeyValuePair<int, string>> GetSupportedRepeatTimes()
        {
            yield return new KeyValuePair<int, string>(0, Resources.none);
            yield return new KeyValuePair<int, string>(1, "1");
            yield return new KeyValuePair<int, string>(2, "2");
            yield return new KeyValuePair<int, string>(3, "3");
            yield return new KeyValuePair<int, string>(5, "5");
            yield return new KeyValuePair<int, string>(10, "10");
            yield return new KeyValuePair<int, string>(int.MaxValue, Resources.unlimited);
        }

        private IEnumerable<KeyValuePair<RepeatAmount, string>> GetSupportedRepeatAmounts()
        {
            yield return new KeyValuePair<RepeatAmount, string>(RepeatAmount.None, Resources.none);
            yield return new KeyValuePair<RepeatAmount, string>(RepeatAmount.OneAyah, "1 " + QuranUtils.GetAyahTitle());
            yield return new KeyValuePair<RepeatAmount, string>(RepeatAmount.ThreeAyah, "3 " + QuranUtils.GetAyahTitle());
            yield return new KeyValuePair<RepeatAmount, string>(RepeatAmount.FiveAyah, "5 " + QuranUtils.GetAyahTitle());
            yield return new KeyValuePair<RepeatAmount, string>(RepeatAmount.TenAyah, "10 " + QuranUtils.GetAyahTitle());
            yield return new KeyValuePair<RepeatAmount, string>(RepeatAmount.Page, Resources.quran_page);
            yield return new KeyValuePair<RepeatAmount, string>(RepeatAmount.Surah, Resources.quran_sura);
            yield return new KeyValuePair<RepeatAmount, string>(RepeatAmount.Rub, Resources.quran_rub3);
            yield return new KeyValuePair<RepeatAmount, string>(RepeatAmount.Juz, Resources.quran_juz2);
        }
    }
}
