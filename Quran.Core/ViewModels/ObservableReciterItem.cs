using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Cirrious.MvvmCross.ViewModels;
using Quran.Core.Utils;
using Quran.Core.Common;
using Quran.Core.Data;

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
            this.Exists = FileUtils.DirectoryExists(item.LocalPath);
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

                base.RaisePropertyChanged(() => Id);
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

                base.RaisePropertyChanged(() => Name);
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

                base.RaisePropertyChanged(() => LocalUrl);
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

                base.RaisePropertyChanged(() => FileName);
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

                base.RaisePropertyChanged(() => IsCompressed);
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

                base.RaisePropertyChanged(() => IsGapless);
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

                base.RaisePropertyChanged(() => ServerUrl);
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

                base.RaisePropertyChanged(() => DatabaseName);
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

                base.RaisePropertyChanged(() => Exists);
            }
        }

        MvxCommand deleteCommand;
        /// <summary>
        /// Returns an undo command
        /// </summary>
        public ICommand DeleteCommand
        {
            get
            {
                if (deleteCommand == null)
                {
                    deleteCommand = new MvxCommand(Delete);
                }
                return deleteCommand;
            }
        }

        MvxCommand navigateCommand;
        /// <summary>
        /// Returns an undo command
        /// </summary>
        public ICommand NavigateCommand
        {
            get
            {
                if (navigateCommand == null)
                {
                    navigateCommand = new MvxCommand(Navigate);
                }
                return navigateCommand;
            }
        }

        public void Delete()
        {
            if (FileUtils.DirectoryExists(this.LocalUrl))
            {
                try
                {
                    FileUtils.DeleteFolder(this.LocalUrl);
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
