using System;
using System.Windows.Input;
using Quran.Core.Utils;
using Quran.Core.Common;
using Quran.Core.Data;
using System.IO;
using System.Threading.Tasks;
using Quran.Core.Properties;
using System.Collections.Generic;

namespace Quran.Core.ViewModels
{
    public class ObservableTranslationItem : DownloadableViewModelBase
    {
        private readonly string _serverUrl;
        private readonly bool _isCompressed;

        public ObservableTranslationItem() { }

        public ObservableTranslationItem(TranslationItem item)
        {
            this.Id = item.Id;
            this.Name = item.Name;
            this.Translator = item.Translator;
            this.Exists = item.Exists;
            this.LocalPath = Path.Combine(FileUtils.GetQuranDatabaseDirectory(), item.Filename);
            _isCompressed = item.Compressed;
            _serverUrl = item.Url;
        }

        public string LocalPath { get; set; }

        private int id;
        public int Id
        {
            get { return id; }
            set
            {
                if (value == id)
                    return;

                id = value;

                base.OnPropertyChanged(() => Id);
            }
        }

        private string name;
        public string Name
        {
            get { return name; }
            set
            {
                if (value == name)
                    return;

                name = value;

                base.OnPropertyChanged(() => Name);
            }
        }

        private string translator;
        public string Translator
        {
            get { return translator; }
            set
            {
                if (value == translator)
                    return;

                translator = value;

                base.OnPropertyChanged(() => Translator);
            }
        }

        private bool exists;
        public bool Exists
        {
            get { return exists; }
            set
            {
                if (value == exists)
                    return;

                exists = value;

                base.OnPropertyChanged(() => Exists);
            }
        }

        ICommand deleteCommand;
        /// <summary>
        /// Returns an delete command
        /// </summary>
        public ICommand DeleteCommand
        {
            get
            {
                if (deleteCommand == null)
                {
                    deleteCommand = new RelayCommand(Delete);
                }
                return deleteCommand;
            }
        }
        
        ICommand downloadCommand;
        /// <summary>
        /// Returns an download command
        /// </summary>
        public ICommand DownloadCommand
        {
            get
            {
                if (downloadCommand == null)
                {
                    downloadCommand = new RelayCommand(Download, () => !Exists);
                }
                return downloadCommand;
            }
        }

        public async void Delete()
        {
            if (await FileUtils.FileExists(LocalPath))
            {
                try
                {
                    await FileUtils.SafeFileDelete(LocalPath);
                }
                catch (Exception ex)
                {
                    QuranApp.NativeProvider.Log("error deleting file " + LocalPath);
                    telemetry.TrackException(ex, new Dictionary<string, string> { { "Scenario", "DeletingTranslationFile" } });
                }
            }
            else
            {
                // Sometimes downloaded translation is kind of corrupted, need a way to delete this
                // corrupted item.
            }

            if (DeleteComplete != null)
                DeleteComplete(this, null);

            if (SettingsUtils.Get<string>(Constants.PREF_ACTIVE_TRANSLATION).StartsWith(Path.GetFileName(LocalPath), 
                StringComparison.Ordinal))
            {
                SettingsUtils.Set<string>(Constants.PREF_ACTIVE_TRANSLATION, string.Empty);
            }
        }

        public async void Download()
        {
            await DownloadSingleFile(_serverUrl, _isCompressed ? LocalPath + ".zip" : LocalPath);
        }
        
        public event EventHandler DeleteComplete;
    }
}
