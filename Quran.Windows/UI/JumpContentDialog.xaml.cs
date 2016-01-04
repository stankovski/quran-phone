using System.Globalization;
using Quran.Core.Common;
using Quran.Core.Data;
using Quran.Core.Utils;
using Windows.UI.Xaml.Controls;

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Quran.Windows.UI
{
    public sealed partial class JumpContentDialog : ContentDialog
    {
        public QuranAyah Ayah { get; set; }
        public int? Page { get; set; }

        public JumpContentDialog()
        {
            this.InitializeComponent();
            for (int i = Constants.SURA_FIRST; i <= Constants.SURA_LAST; i++)
            {
                cbSurahs.Items.Add(i.ToString(CultureInfo.InvariantCulture) + " - " + QuranUtils.GetSurahName(i, false));
            }
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (!string.IsNullOrEmpty(tbPageNumber.Text))
            {
                int page = 0;
                if (int.TryParse(tbPageNumber.Text, NumberStyles.Integer, 
                    CultureInfo.InvariantCulture, out page))
                {
                    Page = page;
                }
            }
            else if (cbSurahs.SelectedIndex >= 0)
            {
                var ayah = cbAyah.SelectedIndex + 1;
                var surah = cbSurahs.SelectedIndex + 1;
                Ayah = new QuranAyah(surah, ayah);
                Page = QuranUtils.GetPageFromAyah(Ayah);
            }
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Page = null;
            Ayah = null;
        }

        private void SyrahSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbSurahs.SelectedItem != null)
            {
                tbPageNumber.Text = "";
                cbAyah.Items.Clear();
                for (int i = 1; i <= QuranUtils.GetSurahNumberOfAyah(cbSurahs.SelectedIndex + 1); i++)
                {
                    cbAyah.Items.Add(i.ToString(CultureInfo.InvariantCulture));
                }
                cbAyah.SelectedIndex = 0;
            }
        }
    }
}
