namespace QuranPhone.ViewModels
{
    public class VerseViewModel : ViewModelBase
    {
        public VerseViewModel()
        {
        }

        public VerseViewModel(string text)
        {
            Text = text;
        }

        public VerseViewModel(string text, string style)
        {
            Text = text;
            StyleName = style;
        }

        #region Properties
        private string styleName;
        public string StyleName
        {
            get { return styleName; }
            set
            {
                if (value == styleName)
                    return;

                styleName = value;

                base.OnPropertyChanged(() => StyleName);
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
        #endregion Properties
    }
}
