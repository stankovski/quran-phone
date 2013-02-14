using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuranPhone.ViewModels
{
    public class VerseViewModel : ViewModelBase
    {
        public VerseViewModel()
        {
            this.QuranTextExists = false;
        }

        #region Properties
        private int surahNumber;
        public int SurahNumber
        {
            get { return surahNumber; }
            set
            {
                if (value == surahNumber)
                    return;

                surahNumber = value;

                base.OnPropertyChanged(() => SurahNumber);
            }
        }

        private int verseNumber;
        public int VerseNumber
        {
            get { return verseNumber; }
            set
            {
                if (value == verseNumber)
                    return;

                verseNumber = value;

                base.OnPropertyChanged(() => VerseNumber);
            }
        }

        private bool isTitle;
        public bool IsTitle
        {
            get { return isTitle; }
            set
            {
                if (value == isTitle)
                    return;

                isTitle = value;

                base.OnPropertyChanged(() => IsTitle);
            }
        }

        private string text;
        public string Text
        {
            get { return text; }
            set
            {
                if (value == text)
                    return;

                text = value;

                base.OnPropertyChanged(() => Text);
            }
        }

        private string quranText;
        public string QuranText
        {
            get { return quranText; }
            set
            {
                if (value == quranText)
                    return;

                quranText = value;

                if (!string.IsNullOrEmpty(quranText))
                    QuranTextExists = true;

                base.OnPropertyChanged(() => QuranText);
            }
        }

        private bool quranTextExists;
        public bool QuranTextExists
        {
            get { return quranTextExists; }
            set
            {
                if (value == quranTextExists)
                    return;

                quranTextExists = value;

                base.OnPropertyChanged(() => QuranTextExists);
            }
        }
        #endregion Properties
    }
}
