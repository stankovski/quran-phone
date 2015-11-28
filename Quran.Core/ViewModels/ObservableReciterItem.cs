using System;
using Quran.Core.Utils;
using Quran.Core.Common;
using Quran.Core.Data;
using System.Threading.Tasks;

namespace Quran.Core.ViewModels
{
    public class ObservableReciterItem : BaseViewModel
    {
        public ObservableReciterItem() { }

        public ObservableReciterItem(ReciterItem item)
            : base()
        {
            this.Id = item.Id;
            this.Name = item.Name;
            this.ServerUrl = item.ServerUrl;
            this.LocalUrl = item.LocalPath;
            this.DatabaseName = item.GaplessDatabasePath;
            this.IsGapless = item.IsGapless;
        }

        public override async Task Initialize()
        {
            this.Exists = await FileUtils.DirectoryExists(LocalUrl);
        }

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

        private string localUrl;
        public string LocalUrl
        {
            get { return localUrl; }
            set
            {
                if (value == localUrl)
                    return;

                localUrl = value;

                base.OnPropertyChanged(() => LocalUrl);
            }
        }

        private string filename;
        public string FileName
        {
            get { return filename; }
            set
            {
                if (value == filename)
                    return;

                filename = value;

                base.OnPropertyChanged(() => FileName);
            }
        }

        private bool isCompressed;
        public bool IsCompressed
        {
            get { return isCompressed; }
            set
            {
                if (value == isCompressed)
                    return;

                isCompressed = value;

                base.OnPropertyChanged(() => IsCompressed);
            }
        }

        private bool isGapless;
        public bool IsGapless
        {
            get { return isGapless; }
            set
            {
                if (value == isGapless)
                    return;

                isGapless = value;

                base.OnPropertyChanged(() => IsGapless);
            }
        }

        private string serverUrl;
        public string ServerUrl
        {
            get { return serverUrl; }
            set
            {
                if (value == serverUrl)
                    return;

                serverUrl = value;

                base.OnPropertyChanged(() => ServerUrl);
            }
        }

        private string databaseName;
        public string DatabaseName
        {
            get { return databaseName; }
            set
            {
                if (value == databaseName)
                    return;

                databaseName = value;

                base.OnPropertyChanged(() => DatabaseName);
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
        
        public async Task Delete()
        {
            if (await FileUtils.DirectoryExists(this.LocalUrl))
            {
                try
                {
                    await FileUtils.DeleteFolder(this.LocalUrl);
                }
                catch
                {
                    QuranApp.NativeProvider.Log("error deleting file " + this.LocalUrl);
                }
            }
            else
            {
                // Sometimes downloaded translation is kind of corrupted, need a way to delete this
                // corrupted item.

            }

            if (DeleteComplete != null)
                DeleteComplete(this, null);

            try
            {
                if (SettingsUtils.Get<string>(Constants.PREF_ACTIVE_QARI) == this.Name)
                {
                    SettingsUtils.Set<string>(Constants.PREF_ACTIVE_QARI, string.Empty);
                }
            }
            catch (Exception)
            {
            }
        }

        public void Navigate()
        {
            if (NavigateRequested != null)
                NavigateRequested(this, null);
        }

        public event EventHandler DeleteComplete;
        public event EventHandler NavigateRequested;
    }
}
