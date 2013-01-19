using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using QuranPhone.Resources;
using QuranPhone.Data;
using System.Windows.Controls;

namespace QuranPhone.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            this.Surahs = new ObservableCollection<ItemViewModel>();
            this.Juz = new ObservableCollection<ItemViewModel>();
            this.Bookmarks = new ObservableCollection<ItemViewModel>();
            this.Tags = new ObservableCollection<ItemViewModel>();
        }

        public ObservableCollection<ItemViewModel> Surahs { get; private set; }
        public ObservableCollection<ItemViewModel> Juz { get; private set; }
        public ObservableCollection<ItemViewModel> Bookmarks { get; private set; }
        public ObservableCollection<ItemViewModel> Tags { get; private set; }

        public bool IsDataLoaded
        {
            get;
            private set;
        }

        /// <summary>
        /// Creates and adds a few ItemViewModel objects into the Items collection.
        /// </summary>
        public void LoadData()
        {
            // Sample data; replace with real data
            loadSuraList();
            loadJuz2List();

            this.IsDataLoaded = true;
        }

        #region Private Methods
        private void loadSuraList()
        {
            int sura = 1;
            int next = 1;

            for (int juz = 1; juz <= Constants.JUZ2_COUNT; juz++)
            {
                Surahs.Add(new ItemViewModel
                {
                    Id = QuranInfo.GetJuzTitle() + " " + juz,
                    PageNumber = QuranInfo.JUZ_PAGE_START[juz - 1],
                    ItemType = ItemViewModelType.Juz
                });
                next = (juz == Constants.JUZ2_COUNT) ? Constants.PAGES_LAST + 1 : QuranInfo.JUZ_PAGE_START[juz];

                while ((sura <= Constants.SURAS_COUNT) && (QuranInfo.SURA_PAGE_START[sura - 1] < next))
                {
                    string title = QuranInfo.GetSuraName(sura, true);
                    Surahs.Add(new ItemViewModel
                    {
                        Id = sura.ToString(),
                        Title = title,
                        Details = QuranInfo.GetSuraListMetaString(sura),
                        PageNumber = QuranInfo.SURA_PAGE_START[sura - 1],
                        ItemType = ItemViewModelType.Sura
                    });
                    sura++;
                }
            }
        }

        private void loadJuz2List()
        {
            Uri[] images = new Uri[] {
                new Uri("/Assets/Images/hizb_full.png", UriKind.Relative),
                new Uri("/Assets/Images/hizb_quarter.png", UriKind.Relative),
                new Uri("/Assets/Images/hizb_half.png", UriKind.Relative),
                new Uri("/Assets/Images/hizb_threequarters.png", UriKind.Relative)
            };
            string[] quarters = QuranInfo.GetSuraQuarters();
            for (int i = 0; i < (8 * Constants.JUZ2_COUNT); i++)
            {
                int[] pos = QuranInfo.QUARTERS[i];
                int page = QuranInfo.GetPageFromSuraAyah(pos[0], pos[1]);

                if (i % 8 == 0)
                {
                    int juz = 1 + (i / 8);
                    Juz.Add(new ItemViewModel
                    {
                        Id = QuranInfo.GetJuzTitle() + " " + juz,
                        PageNumber = QuranInfo.JUZ_PAGE_START[juz - 1],
                        ItemType = ItemViewModelType.Juz
                    });
                }
                string verseString = AppResources.quran_ayah + " " + pos[1];
                Juz.Add(new ItemViewModel 
                { 
                    Title = quarters[i], 
                    Details = QuranInfo.GetSuraName(pos[0], true) + ", " + verseString, 
                    PageNumber = page,
                    Image = images[i % 4],
                    ItemType = ItemViewModelType.Sura
                });

                if (i % 4 == 0)
                    Juz[Juz.Count - 1].Id = (1 + (i / 4)).ToString("0");
            }
        }
        #endregion
    }
}