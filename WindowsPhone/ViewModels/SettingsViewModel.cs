using System;
using System.Windows.Input;
using Microsoft.Phone.Tasks;
using QuranPhone.Data;
using QuranPhone.UI;
using QuranPhone.Utils;
using System.IO;

namespace QuranPhone.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
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
                SettingsUtils.Set(Constants.PREF_ARABIC_TEXT_SIZE, value * Constants.ARABIC_FONT_SCALE_RELATIVE_TO_TRANSLATION);
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

        #endregion Properties

        public void LoadData()
        {
            var translation = SettingsUtils.Get<string>(Constants.PREF_ACTIVE_TRANSLATION);
            if (!string.IsNullOrEmpty(translation) && translation.Contains("|"))
                ActiveTranslation = translation.Split('|')[1];
            else
                ActiveTranslation = "None";

            TextSize = SettingsUtils.Get<int>(Constants.PREF_TRANSLATION_TEXT_SIZE);
            ShowArabicInTranslation = SettingsUtils.Get<bool>(Constants.PREF_SHOW_ARABIC_IN_TRANSLATION);

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

        private void ContactUs()
        {
            var email = new EmailComposeTask();
            email.To = "denis.stankovski@gmail.com";
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
    }
}
