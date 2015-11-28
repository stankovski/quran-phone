using System;
using System.Windows.Input;
using Quran.Core.Utils;
using Quran.Core.Common;
using Quran.Core.Data;
using System.IO;
using System.Threading.Tasks;

namespace Quran.Core.ViewModels
{
    public class ObservableTranslationItem : DownloadableViewModelBase
    {
        private readonly string _serverUrl;
        private readonly string _localPath;

        public ObservableTranslationItem() { }

        public ObservableTranslationItem(TranslationItem item)
            : base()
        {
            this.Id = item.Id;
            this.Name = item.Name;
            this.Translator = item.Translator;
            this.Exists = item.Exists;
            _serverUrl = item.Url;
            _localPath = Path.Combine(FileUtils.RunSync(() => FileUtils.GetQuranDatabaseDirectory()), item.Filename);
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

        public async Task Delete()
        {
            if (await FileUtils.FileExists(_localPath))
            {
                try
                {
                    await FileUtils.DeleteFile(_localPath);
                }
                catch
                {
                    QuranApp.NativeProvider.Log("error deleting file " + _localPath);
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
                if (SettingsUtils.Get<string>(Constants.PREF_ACTIVE_TRANSLATION).StartsWith(_localPath))
                {
                    SettingsUtils.Set<string>(Constants.PREF_ACTIVE_TRANSLATION, string.Empty);
                }
            }
            catch (Exception)
            {
            }
        }

        public async Task<bool> Download()
        {
            return await DownloadSingleFile(_serverUrl, _localPath);
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
