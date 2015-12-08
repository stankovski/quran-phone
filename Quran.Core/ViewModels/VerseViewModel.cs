// --------------------------------------------------------------------------------------------------------------------
// <summary>
//    Defines the VerseViewModel type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
using System.Threading.Tasks;
using Quran.Core.Data;
using Quran.Core.Utils;

namespace Quran.Core.ViewModels
{
    /// <summary>
    /// Define the VerseViewModel type.
    /// </summary>
    public class VerseViewModel : BaseViewModel
    {
        public VerseViewModel(DetailsViewModel parent)
        {
            Parent = parent;
            FontSize = SettingsUtils.Get<double>(Constants.PREF_TRANSLATION_TEXT_SIZE);
            ArabicFontSize = FontSize * Constants.ARABIC_FONT_SCALE_RELATIVE_TO_TRANSLATION;
        }
        
        #region Properties
        public DetailsViewModel Parent { get; set; }

        public bool ContainsArabicText {
            get
            {
                return ArabicText != null;
            }
        }

        public double FontSize { get; set; }
        public double ArabicFontSize { get; set; }

        public string AyahName { get
            {
                return string.Format("{0}:{1}", Surah, Ayah);
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

        private string arabicText;
        public string ArabicText
        {
            get { return arabicText; }
            set
            {
                if (value == arabicText)
                    return;

                arabicText = value;

                base.OnPropertyChanged(() => ArabicText);
            }
        }

        private int surah;
        public int Surah
        {
            get { return surah; }
            set
            {
                if (value == surah)
                    return;

                surah = value;

                base.OnPropertyChanged(() => Surah);
            }
        }

        private int ayah;
        public int Ayah
        {
            get { return ayah; }
            set
            {
                if (value == ayah)
                    return;

                ayah = value;

                base.OnPropertyChanged(() => Ayah);
            }
        }

        private bool isHeader;
        public bool IsHeader
        {
            get { return isHeader; }
            set
            {
                if (value == isHeader)
                    return;

                isHeader = value;

                base.OnPropertyChanged(() => IsHeader);
            }
        }
        #endregion Properties

        public override Task Initialize()
        {
            return Refresh();
        }

        public override Task Refresh()
        {
            return Task.FromResult(0);
        }
    }
}
