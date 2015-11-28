// --------------------------------------------------------------------------------------------------------------------
// <summary>
//    Defines the TranslationslistViewModel type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Quran.Core.Common;
using Quran.Core.Utils;

namespace Quran.Core.ViewModels
{
    /// <summary>
    /// Define the TranslationslistViewModel type.
    /// </summary>
    public class RecitersListViewModel : BaseViewModel
    {
        public RecitersListViewModel()
        {
            this.IsDataLoaded = false;
            this.AvailableReciters = new ObservableCollection<ObservableReciterItem>();
            this.AvailableReciters.CollectionChanged += AvailableTranslationsCollectionChanged;
        }

        #region Properties
        public ObservableCollection<ObservableReciterItem> AvailableReciters { get; private set; }

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

        #endregion Properties

        #region Public methods
        public override Task Initialize()
        {
            return Task.FromResult(0);
        }

        public async Task LoadData()
        {
            var qariNames = AudioUtils.GetReciterItems().Where(r => !r.IsGapless);

            foreach (var item in qariNames)
            {
                var observableItem = new ObservableReciterItem(item);
                await observableItem.Initialize();
                this.AvailableReciters.Add(observableItem);
            }

            this.IsDataLoaded = true;
        }
        #endregion Public methods

        #region Event handlers
        void AvailableTranslationsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add ||
                e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace)
            {
                foreach (ObservableReciterItem item in e.NewItems)
                {
                    item.DeleteComplete += TranslationDeleteComplete;
                    item.NavigateRequested += TranslationNavigateRequested;
                }
            }
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove ||
                e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace ||
                e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                foreach (ObservableReciterItem item in e.OldItems)
                {
                    item.DeleteComplete -= TranslationDeleteComplete;
                    item.NavigateRequested -= TranslationNavigateRequested;
                }
            }
        }

        private void TranslationDeleteComplete(object sender, EventArgs e)
        {
            var translation = sender as ObservableReciterItem;
            if (translation == null)
                return;
            translation.Exists = false;

            // Hack to update list after download / delete completed
            AvailableReciters.Remove(translation);
            AvailableReciters.Add(translation);
        }

        private void TranslationNavigateRequested(object sender, EventArgs e)
        {
            var translation = sender as ObservableReciterItem;
            if (translation == null)
                return;
            if (NavigateRequested != null)
                NavigateRequested(sender, e);
        }
        #endregion

        public event EventHandler NavigateRequested;
    }
}
