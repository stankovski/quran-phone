using System;
using System.Windows.Input;
using Microsoft.Phone.Tasks;
using QuranPhone.Data;
using QuranPhone.UI;
using QuranPhone.Utils;

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

                base.OnPropertyChanged(() => TextSize);
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

        #endregion Properties

        public void LoadData()
        {
            var translation = SettingsUtils.Get<string>(Constants.PREF_ACTIVE_TRANSLATION);
            if (!string.IsNullOrEmpty(translation) && translation.Contains("|"))
                ActiveTranslation = translation.Split('|')[1];
            else
                ActiveTranslation = "None";

            TextSize = SettingsUtils.Get<int>(Constants.PREF_TRANSLATION_TEXT_SIZE);
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
