using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using QuranPhone.Common;
using QuranPhone.UI;
using QuranPhone.Utils;

namespace QuranPhone.ViewModels
{
    public class TranslationsListViewModel : ViewModelBase
    {
        public TranslationsListViewModel()
        {
            IsDataLoaded = false;
            AvailableTranslations = new ObservableCollection<ObservableTranslationItem>();
            AvailableTranslations.CollectionChanged += AvailableTranslationsCollectionChanged;
        }

        #region Properties

        private bool _anyTranslationsDownloaded;
        private bool _isDataLoaded;
        public ObservableCollection<ObservableTranslationItem> AvailableTranslations { get; private set; }

        public bool IsDataLoaded
        {
            get { return _isDataLoaded; }
            set
            {
                _isDataLoaded = value;
                base.OnPropertyChanged(() => IsDataLoaded);
            }
        }

        public bool AnyTranslationsDownloaded
        {
            get { return _anyTranslationsDownloaded; }
            set
            {
                _anyTranslationsDownloaded = value;
                base.OnPropertyChanged(() => AnyTranslationsDownloaded);
            }
        }

        #endregion Properties

        #region Public methods

        public async void LoadData()
        {
            IEnumerable<TranslationItem> list = await TranslationListTask.DownloadTranslations(true, "tag");
            if (list == null)
            {
                return;
            }

            foreach (TranslationItem item in list)
            {
                AvailableTranslations.Add(new ObservableTranslationItem(item));
            }

            IsDataLoaded = true;
        }

        #endregion Public methods

        #region Event handlers

        private void AvailableTranslationsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Replace)
            {
                foreach (ObservableTranslationItem item in e.NewItems)
                {
                    item.DownloadComplete += TranslationDownloadComplete;
                    item.DeleteComplete += TranslationDeleteComplete;
                    item.NavigateRequested += TranslationNavigateRequested;
                }
            }
            if (e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Replace ||
                e.Action == NotifyCollectionChangedAction.Reset)
            {
                foreach (ObservableTranslationItem item in e.OldItems)
                {
                    item.DownloadComplete -= TranslationDownloadComplete;
                    item.DeleteComplete -= TranslationDeleteComplete;
                    item.NavigateRequested -= TranslationNavigateRequested;
                }
            }
        }

        private void TranslationDownloadComplete(object sender, EventArgs e)
        {
            var translation = sender as ObservableTranslationItem;
            if (translation == null)
            {
                return;
            }
            translation.Exists = true;

            // Hack to update list after download / delete completed
            AvailableTranslations.Remove(translation);
            AvailableTranslations.Add(translation);
        }

        private void TranslationDeleteComplete(object sender, EventArgs e)
        {
            var translation = sender as ObservableTranslationItem;
            if (translation == null)
            {
                return;
            }
            translation.Exists = false;

            AvailableTranslations.Remove(translation);
            AvailableTranslations.Add(translation);
        }

        private void TranslationNavigateRequested(object sender, EventArgs e)
        {
            var translation = sender as ObservableTranslationItem;
            if (translation == null)
            {
                return;
            }
            if (NavigateRequested != null)
            {
                NavigateRequested(sender, e);
            }
        }

        #endregion

        public event EventHandler NavigateRequested;
    }
}