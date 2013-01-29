using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using QuranPhone.Resources;
using QuranPhone.Data;
using System.Windows.Controls;
using QuranPhone.Common;
using QuranPhone.Utils;

namespace QuranPhone.ViewModels
{
    public class TranslationsListViewModel : ViewModelBase
    {
        public TranslationsListViewModel()
        {
            this.IsDataLoaded = false;
            this.AvailableTranslations = new ObservableCollection<ObservableTranslationItem>();
            this.DownloadedTranslations = new ObservableCollection<ObservableTranslationItem>();
            this.DownloadedTranslations.CollectionChanged += DownloadedTranslations_CollectionChanged;
            this.AvailableTranslations.CollectionChanged += AvailableTranslations_CollectionChanged;
        }

        public ObservableCollection<ObservableTranslationItem> AvailableTranslations { get; private set; }
        public ObservableCollection<ObservableTranslationItem> DownloadedTranslations { get; private set; }

        private bool isDataLoaded;
        public bool IsDataLoaded
        {
            get { return isDataLoaded; }
            set
            {
                if (value == isDataLoaded)
                    return;

                isDataLoaded = value;

                base.OnPropertyChanged(() => IsDataLoaded);
            }
        }

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
        
        /// <summary>
        /// Creates and adds a few ItemViewModel objects into the Items collection.
        /// </summary>
        public async void LoadData()
        {
            // Sample data; replace with real data

            var list = await TranslationListTask.DownloadTranslations(true, "tag");
            foreach (var item in list)
            {
                if (item.Exists)
                    this.DownloadedTranslations.Add(new ObservableTranslationItem(item));
                else
                    this.AvailableTranslations.Add(new ObservableTranslationItem(item));
            }
            
            this.IsDataLoaded = true;
        }

        #region Private Methods
        private void DownloadedTranslations_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (DownloadedTranslations.Count > 0)
                this.AnyTranslationsDownloaded = true;
            else
                this.AnyTranslationsDownloaded = false;

            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add ||
                e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace)
            {
                foreach (ObservableTranslationItem item in e.NewItems)
                {
                    item.DeleteComplete += TranslationDeleteComplete;
                }
            }
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove ||
                e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace ||
                e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                foreach (ObservableTranslationItem item in e.OldItems)
                {
                    item.DeleteComplete -= TranslationDeleteComplete;
                }
            }  
        }

        void AvailableTranslations_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add ||
                e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace)
            {
                foreach (ObservableTranslationItem item in e.NewItems)
                {
                    item.DownloadComplete += TranslationDownloadComplete;
                }
            }
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove ||
                e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace ||
                e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                foreach (ObservableTranslationItem item in e.OldItems)
                {
                    item.DownloadComplete -= TranslationDownloadComplete;
                }
            }            
        }

        private void TranslationDownloadComplete(object sender, EventArgs e)
        {
            if (sender == null)
                return;

            var index = AvailableTranslations.IndexOf(sender as ObservableTranslationItem);
            if (index >= 0)
            {
                var item = AvailableTranslations[index];
                DownloadedTranslations.Add(item);
                AvailableTranslations.Remove(item);
            }
        }

        private void TranslationDeleteComplete(object sender, EventArgs e)
        {
            if (sender == null)
                return;

            var index = DownloadedTranslations.IndexOf(sender as ObservableTranslationItem);
            if (index >= 0)
            {
                var item = DownloadedTranslations[index];
                AvailableTranslations.Add(item);
                DownloadedTranslations.Remove(item);
            }
        }
        #endregion
    }
}