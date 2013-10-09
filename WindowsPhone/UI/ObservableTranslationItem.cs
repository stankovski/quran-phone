using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using QuranPhone.Common;
using QuranPhone.Data;
using QuranPhone.Utils;
using QuranPhone.ViewModels;

namespace QuranPhone.UI
{
    public class ObservableTranslationItem : DownloadableViewModelBase
    {
        private RelayCommand deleteCommand;
        private bool exists;
        private int id;
        private string name;
        private RelayCommand navigateCommand;
        private string translator;

        public ObservableTranslationItem() {}

        public ObservableTranslationItem(TranslationItem item)
        {
            Id = item.Id;
            Name = item.Name;
            Translator = item.Translator;
            ServerUrl = item.Url;
            FileName = item.Filename;
            Exists = item.Exists;
            LocalUrl = Path.Combine(QuranFileUtils.GetQuranDatabaseDirectory(false, true), FileName);
            IsCompressed = item.Compressed;
        }

        public int Id
        {
            get { return id; }
            set
            {
                if (value == id)
                {
                    return;
                }

                id = value;

                base.OnPropertyChanged(() => Id);
            }
        }

        public string Name
        {
            get { return name; }
            set
            {
                if (value == name)
                {
                    return;
                }

                name = value;

                base.OnPropertyChanged(() => Name);
            }
        }

        public string Translator
        {
            get { return translator; }
            set
            {
                if (value == translator)
                {
                    return;
                }

                translator = value;

                base.OnPropertyChanged(() => Translator);
            }
        }

        public bool Exists
        {
            get { return exists; }
            set
            {
                if (value == exists)
                {
                    return;
                }

                exists = value;

                base.OnPropertyChanged(() => Exists);
            }
        }

        /// <summary>
        ///     Returns an undo command
        /// </summary>
        public ICommand DeleteCommand
        {
            get
            {
                if (deleteCommand == null)
                {
                    deleteCommand = new RelayCommand(param => Delete());
                }
                return deleteCommand;
            }
        }

        /// <summary>
        ///     Returns an undo command
        /// </summary>
        public ICommand NavigateCommand
        {
            get
            {
                if (navigateCommand == null)
                {
                    navigateCommand = new RelayCommand(param => Navigate(), canExecute => Exists);
                }
                return navigateCommand;
            }
        }

        public void Delete()
        {
            if (QuranFileUtils.FileExists(LocalUrl))
            {
                try
                {
                    QuranFileUtils.DeleteFile(LocalUrl);
                }
                catch
                {
                    MessageBox.Show("error deleting file " + LocalUrl);
                }
            }

            if (DeleteComplete != null)
            {
                DeleteComplete(this, null);
            }

            try
            {
                if (SettingsUtils.Get<string>(Constants.PrefActiveTranslation).StartsWith(FileName))
                {
                    SettingsUtils.Set(Constants.PrefActiveTranslation, string.Empty);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        public void Navigate()
        {
            if (NavigateRequested != null)
            {
                NavigateRequested(this, null);
            }
        }

        public event EventHandler DeleteComplete;
        public event EventHandler NavigateRequested;
    }
}