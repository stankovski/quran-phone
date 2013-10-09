namespace QuranPhone.ViewModels
{
    public class VerseViewModel : ViewModelBase
    {
        public VerseViewModel() {}

        public VerseViewModel(string text)
        {
            Text = text;
        }

        public VerseViewModel(string text, string style)
        {
            Text = text;
            StyleName = style;
        }

        public VerseViewModel(string text, string style, int surah, int ayah)
        {
            Text = text;
            StyleName = style;
            Surah = surah;
            Ayah = ayah;
        }

        #region Properties

        private int _ayah;
        private string _styleName;
        private int _surah;
        private string _text;

        public string StyleName
        {
            get { return _styleName; }
            set
            {
                _styleName = value;
                base.OnPropertyChanged(() => StyleName);
            }
        }

        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                base.OnPropertyChanged(() => Text);
            }
        }

        public int Surah
        {
            get { return _surah; }
            set
            {
                _surah = value;
                base.OnPropertyChanged(() => Surah);
            }
        }

        public int Ayah
        {
            get { return _ayah; }
            set
            {
                _ayah = value;
                base.OnPropertyChanged(() => Ayah);
            }
        }

        #endregion Properties
    }
}