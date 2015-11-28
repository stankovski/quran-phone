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

        public VerseViewModel(string text, string style, int surah, int ayah)
        {
            Text = text;
            StyleName = style;
            Surah = surah;
            Ayah = ayah;
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
        #endregion Properties

        public override Task Initialize()
        {
            return Task.FromResult(0);
        }
    }
}
