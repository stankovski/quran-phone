// --------------------------------------------------------------------------------------------------------------------
// <summary>
//    Defines the TranslationslistViewModel type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Quran.Core.Properties;
using Quran.Core.Utils;

namespace Quran.Core.ViewModels
{
    public class TranslationsItemGroup
    {
        public TranslationsItemGroup(string title)
        {
            Title = title;
            Translations = new ObservableCollection<ObservableTranslationItem>();
        }

        public string Title { get; set; }

        public ObservableCollection<ObservableTranslationItem> Translations { get; private set; }
    }
    /// <summary>
    /// Define the TranslationslistViewModel type.
    /// </summary>
    public class TranslationsListViewModel : BaseViewModel
    {
        private TranslationsItemGroup _downloadedGroup = new TranslationsItemGroup(AppResources.downloaded_translations);
        private TranslationsItemGroup _availableGroup = new TranslationsItemGroup(AppResources.available_translations);

        public TranslationsListViewModel()
        {
            _downloadedGroup.Translations.CollectionChanged += AvailableTranslationsCollectionChanged;
            _availableGroup.Translations.CollectionChanged += AvailableTranslationsCollectionChanged;
            Groups = new ObservableCollection<TranslationsItemGroup>();
            Groups.Add(_downloadedGroup);
            Groups.Add(_availableGroup);
        }

        #region Properties
        public ObservableCollection<TranslationsItemGroup> Groups { get; private set; }

        private bool anyTranslationsDownloaded;
        public bool AnyTranslationsDownloaded
        {
            get { return anyTranslationsDownloaded; }
            set
            {
                if (value == anyTranslationsDownloaded)
                    return;

                anyTranslationsDownloaded = value;

                base.OnPropertyChanged(() => AnyTranslationsDownloaded);
            }
        }
        #endregion Properties

        #region Public methods
        public override async Task Initialize()
        {
            if (_downloadedGroup.Translations.Count == 0 && _availableGroup.Translations.Count == 0)
            {
                await Refresh();
            }
        }

        public override async Task Refresh()
        {
            this.IsLoading = true;

            var list = await TranslationListTask.DownloadTranslations(true, "tag");
            if (list == null)
                return;

            _availableGroup.Translations.Clear();
            _downloadedGroup.Translations.Clear();
            foreach (var item in list)
            {
                var translationItem = new ObservableTranslationItem(item);
                await translationItem.Initialize();
                if (!translationItem.Exists)
                {
                    _availableGroup.Translations.Add(translationItem);
                }
                else
                {
                    _downloadedGroup.Translations.Add(translationItem);
                }
            }

            this.IsLoading = false;
        }

        #endregion Public methods

        #region Event handlers
        void AvailableTranslationsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add ||
                e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace)
            {
                foreach (ObservableTranslationItem item in e.NewItems)
                {
                    item.DownloadComplete += TranslationDownloadComplete;
                    item.DeleteComplete += TranslationDeleteComplete;
                }
            }
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove ||
                e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace ||
                e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                if (e.OldItems != null)
                {
                    foreach (ObservableTranslationItem item in e.OldItems)
                    {
                        item.DownloadComplete -= TranslationDownloadComplete;
                        item.DeleteComplete -= TranslationDeleteComplete;
                    }
                }
            }            
        }

        private void TranslationDownloadComplete(object sender, EventArgs e)
        {
            var translation = sender as ObservableTranslationItem;
            if (translation == null)
                return;
            translation.Exists = true;
            
            if (_availableGroup.Translations.Contains(translation))
            {
                _availableGroup.Translations.Remove(translation);
                _downloadedGroup.Translations.Add(translation);
            }
        }

        private void TranslationDeleteComplete(object sender, EventArgs e)
        {
            var translation = sender as ObservableTranslationItem;
            if (translation == null)
                return;
            translation.Exists = false;

            if (_downloadedGroup.Translations.Contains(translation))
            {
                _downloadedGroup.Translations.Remove(translation);
                _availableGroup.Translations.Add(translation);
            }
        }        
        #endregion
    }
}
