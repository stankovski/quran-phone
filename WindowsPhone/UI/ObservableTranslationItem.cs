using Microsoft.Phone.BackgroundTransfer;
using QuranPhone.Common;
using QuranPhone.Data;
using QuranPhone.Utils;
using QuranPhone.ViewModels;
using System;
using System.IO;
using System.Windows.Input;

namespace QuranPhone.UI
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
            this.LocalUrl = Path.Combine(QuranFileUtils.GetQuranDatabaseDirectory(false, true), this.FileName);
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

        RelayCommand deleteCommand;
        /// <summary>
        /// Returns an undo command
        /// </summary>
        public ICommand DeleteCommand
        {
            get
            {
                if (deleteCommand == null)
                {
                    deleteCommand = new RelayCommand(
                        param => this.Delete()
                        );
                }
                return deleteCommand;
            }
        }

        RelayCommand navigateCommand;
        /// <summary>
        /// Returns an undo command
        /// </summary>
        public ICommand NavigateCommand
        {
            get
            {
                if (navigateCommand == null)
                {
                    navigateCommand = new RelayCommand(
                        param => this.Navigate(),
                        canExecute => this.Exists
                        );
                }
                return navigateCommand;
            }
        }

        public void Delete()
        {
            if (QuranFileUtils.FileExists(this.LocalUrl))
            {
                try
                {
                    QuranFileUtils.DeleteFile(this.LocalUrl);
                }
                catch
                {
                    Console.WriteLine("error deleting file " + this.LocalUrl);
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
