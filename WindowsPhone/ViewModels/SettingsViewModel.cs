using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuranPhone.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
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

        public void LoadData()
        {
            //
        }
    }
}
