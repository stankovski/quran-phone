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
    public class ObservableTranslationItem : DownloadableViewModelBase
    {
        public ObservableTranslationItem() { }

        public ObservableTranslationItem(TranslationItem item)
            : base()
        {
            this.Id = item.Id;
            this.Name = item.Name;
            this.Translator = item.Translator;
            this.ServerUrl = item.Url;
            this.FileName = item.Filename;
            this.Exists = item.Exists;
            this.LocalUrl = PathHelper.Combine(FileUtils.GetQuranDatabaseDirectory(false, true), this.FileName);
            this.IsCompressed = item.Compressed;
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

        private string translator;
        public string Translator
        {
            get { return translator; }
            set
            {
                if (value == translator)
                    return;

                translator = value;

                base.RaisePropertyChanged(() => Translator);
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
                    navigateCommand = new MvxCommand(Navigate, () => this.Exists);
                }
                return navigateCommand;
            }
        }

        public void Delete()
        {
            if (FileUtils.FileExists(this.LocalUrl))
            {
                try
                {
                    FileUtils.DeleteFile(this.LocalUrl);
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
                if (SettingsUtils.Get<string>(Constants.PREF_ACTIVE_TRANSLATION).StartsWith(this.FileName))
                {
                    SettingsUtils.Set<string>(Constants.PREF_ACTIVE_TRANSLATION, string.Empty);
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
