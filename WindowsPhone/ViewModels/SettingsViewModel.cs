using QuranPhone.Data;
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
    }
}
