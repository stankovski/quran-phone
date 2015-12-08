// --------------------------------------------------------------------------------------------------------------------
// <summary>
//    Defines the VerseViewModel type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
using System.Threading.Tasks;

namespace Quran.Core.ViewModels
{
    /// <summary>
    /// Define the VerseViewModel type.
    /// </summary>
    public class VerseViewModel : BaseViewModel
    {
        public VerseViewModel() { }

        public VerseViewModel(DetailsViewModel parent)
        {
            Parent = parent;
        }
        
        #region Properties
        public DetailsViewModel Parent { get; set; }

        public string ArabicText { get; set; }
        public bool ContainsArabicText {
            get
            {
                return ArabicText != null;
            }
        }
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
