using Microsoft.Phone.BackgroundTransfer;
using QuranPhone.UI;
using QuranPhone.Utils;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace QuranPhone.Common
{
    [Table("translations")]
    public class TranslationItem
    {
        [Column("id"), PrimaryKey]
        public int Id { get; set; }
        [Column("name")]
        public string Name { get; set; }
        [Column("translator")]
        public string Translator { get; set; }
        [Column("filename")]
        public string Filename { get; set; }
        [Column("url")]
        public string Url { get; set; }
        [Ignore]
        public bool Exists { get; set; }
        [Ignore]
        public int LatestVersion { get; set; }
        [Column("version")]
        public int LocalVersion { get; set; }
        [Ignore]
        public bool IsSeparator { get; set; }

        public TranslationItem()
        { }

        public TranslationItem(string name)
        {
            Name = name;
            IsSeparator = false;
        }

        public TranslationItem(int id, string name, string translator,
                               int latestVersion, string filename, string url,
                               bool exists)
        {
            Id = id;
            Name = name;
            Translator = translator;
            Filename = filename;
            Url = url;
            Exists = exists;
            LatestVersion = latestVersion;
            IsSeparator = false;
        }        
    }

    public class ObservableTranslationItem : QuranPhone.ViewModels.DownloadableViewModelBase
    {
        public ObservableTranslationItem() { }

        public ObservableTranslationItem(TranslationItem item) : base()
        {
            this.Name = item.Name;
            this.Translator = item.Translator;
            this.ServerUrl = item.Url;
            this.FileName = item.Filename;
            this.LocalUrl = Path.Combine(QuranFileUtils.GetQuranDatabaseDirectory(false, true), this.FileName);
        }

        private BackgroundTransferRequest downloadRequest;

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

                if (value)
                    this.CanDownload = false;

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

        public void Delete()
        {
            if (QuranFileUtils.FileExists(this.LocalUrl))
            {
                QuranFileUtils.DeleteFile(this.LocalUrl);
                if (DeleteComplete != null)
                    DeleteComplete(this, null);
            }
        }

        public event EventHandler DeleteComplete;
    }
}
