using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using QuranPhone.Data;
using QuranPhone.Resources;
using QuranPhone.UI;
using QuranPhone.Utils;
using System.IO;

namespace QuranPhone.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        public SettingsViewModel()
        {
            SupportedLanguages = new ObservableCollection<KeyValuePair<string, string>>(GetSupportedLanguages());
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
                    PhoneUtils.ToggleIdleMode();
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
                    MessageBox.Show(AppResources.please_restart);
                }

                // saving to setting utils
                SettingsUtils.Set(Constants.PREF_CULTURE_OVERRIDE, value.Key);

                base.OnPropertyChanged(() => SelectedLanguage);
            }
        }

        public ObservableCollection<KeyValuePair<string, string>> SupportedLanguages { get; private set; }

        RelayCommand generate;
        /// <summary>
        /// Returns an download command
        /// </summary>
        public ICommand Generate
        {
            get
            {
                if (generate == null)
                {
                    generate = new RelayCommand(
                        param => this.GenerateDua(),
                        param => this.CanGenerateDuaDownload
                        );
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
        RelayCommand navigateCommand;
        /// <summary>
        /// Returns a navigate command
        /// </summary>
        public ICommand NavigateCommand
        {
            get
            {
                if (navigateCommand == null)
                {
                    navigateCommand = new RelayCommand(
                        param => this.Navigate(param)
                    );
                }
                return navigateCommand;
            }
        }
        RelayCommand contactUsCommand;
        /// <summary>
        /// Returns a navigate command
        /// </summary>
        public ICommand ContactUsCommand
        {
            get
            {
                if (contactUsCommand == null)
                {
                    contactUsCommand = new RelayCommand(
                        param => this.ContactUs()
                    );
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

            SelectedLanguage = SupportedLanguages.FirstOrDefault(kv => kv.Key == SettingsUtils.Get<string>(Constants.PREF_CULTURE_OVERRIDE));
            TextSize = SettingsUtils.Get<int>(Constants.PREF_TRANSLATION_TEXT_SIZE);
            ShowArabicInTranslation = SettingsUtils.Get<bool>(Constants.PREF_SHOW_ARABIC_IN_TRANSLATION);
            PreventPhoneFromSleeping = SettingsUtils.Get<bool>(Constants.PREF_PREVENT_SLEEP);
            KeepInfoOverlay = SettingsUtils.Get<bool>(Constants.PREF_KEEP_INFO_OVERLAY);
            NightMode = SettingsUtils.Get<bool>(Constants.PREF_NIGHT_MODE);

            if (QuranFileUtils.FileExists(Path.Combine(QuranFileUtils.GetQuranDatabaseDirectory(false),
                                                       QuranFileUtils.QURAN_ARABIC_DATABASE)))
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
            var email = new EmailComposeTask();
            email.To = "quran.phone@gmail.com";
            email.Subject = "Email from QuranPhone";
            email.Show();
        }

        private void Navigate(object param)
        {
            var link = param as string;
            if (link != null)
            {
                var task = new WebBrowserTask() { Uri = new Uri(link) };
                task.Show();
            }
        }

        private IEnumerable<KeyValuePair<string, string>> GetSupportedLanguages()
        {
            yield return new KeyValuePair<string, string>("", "Default");
            var cultures = new string[] {"ar", "id", "ru"};
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
