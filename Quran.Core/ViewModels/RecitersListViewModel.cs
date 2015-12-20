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
using Quran.Core.Properties;
using Quran.Core.Utils;

namespace Quran.Core.ViewModels
{
    public class ReciterItemGroup
    {
        public ReciterItemGroup(string title)
        {
            Title = title;
            Reciters = new ObservableCollection<ObservableReciterItem>();
        }

        public string Title { get; set; }

        public ObservableCollection<ObservableReciterItem> Reciters { get; private set; }
    }

    /// <summary>
    /// Define the TranslationslistViewModel type.
    /// </summary>
    public class RecitersListViewModel : BaseViewModel
    {
        private ReciterItemGroup _downloadedGroup = new ReciterItemGroup(Resources.downloaded_reciters);
        private ReciterItemGroup _availableGroup = new ReciterItemGroup(Resources.available_reciters);

        public RecitersListViewModel()
        {
            _downloadedGroup.Reciters.CollectionChanged += AvailableTranslationsCollectionChanged;
            _availableGroup.Reciters.CollectionChanged += AvailableTranslationsCollectionChanged;
            Groups = new ObservableCollection<ReciterItemGroup>();
            Groups.Add(_downloadedGroup);
            Groups.Add(_availableGroup);
        }

        #region Properties
        public ObservableCollection<ReciterItemGroup> Groups { get; private set; }
        #endregion Properties

        #region Public methods
        public override async Task Initialize()
        {
            if (_downloadedGroup.Reciters.Count == 0 && _availableGroup.Reciters.Count == 0)
            {
                await Refresh();
            }
        }

        public override async Task Refresh()
        {
            this.IsLoading = true;

            var qariNames = AudioUtils.GetReciterItems()
                .Where(r => !r.IsGapless);

            _availableGroup.Reciters.Clear();
            _downloadedGroup.Reciters.Clear();

            foreach (var item in qariNames)
            {
                var translationItem = new ObservableReciterItem(item);
                await translationItem.Initialize();
                if (!translationItem.Exists)
                {
                    _availableGroup.Reciters.Add(translationItem);
                }
                else
                {
                    _downloadedGroup.Reciters.Add(translationItem);
                }
            }

            this.IsLoading = true;
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
                    item.DeleteComplete += RecitationDeleteComplete;
                }
            }
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove ||
                e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace ||
                e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                foreach (ObservableReciterItem item in e.OldItems)
                {
                    item.DeleteComplete -= RecitationDeleteComplete;
                }
            }
        }

        private void RecitationDeleteComplete(object sender, EventArgs e)
        {
            var recitation = sender as ObservableReciterItem;
            if (recitation == null)
                return;
            recitation.Exists = false;

            if (_downloadedGroup.Reciters.Contains(recitation))
            {
                _downloadedGroup.Reciters.Remove(recitation);
                _availableGroup.Reciters.Add(recitation);
            }
        }
        #endregion Event handlers
    }
}
