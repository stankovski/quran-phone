using QuranPhone.UI;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;

namespace QuranPhone.ViewModels
{
    public class TranslationItemViewModel : ViewModelBase
    {
        private string id;
        public string Id
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


        private string title;
        public string Title
        {
            get { return title; }
            set
            {
                if (value == title)
                    return;

                title = value;

                base.OnPropertyChanged(() => Title);
            }
        }

        private Uri url;
        public Uri Url
        {
            get { return url; }
            set
            {
                if (value == url)
                    return;

                url = value;

                base.OnPropertyChanged(() => Url);
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

        private string fileName;
        public string FileName
        {
            get { return fileName; }
            set
            {
                if (value == fileName)
                    return;

                fileName = value;

                base.OnPropertyChanged(() => FileName);
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

        private int latestVersion;
        public int LatestVersion
        {
            get { return latestVersion; }
            set
            {
                if (value == latestVersion)
                    return;

                latestVersion = value;

                base.OnPropertyChanged(() => LatestVersion);
            }
        }

        private int localVersion;
        public int LocalVersion
        {
            get { return localVersion; }
            set
            {
                if (value == localVersion)
                    return;

                localVersion = value;

                base.OnPropertyChanged(() => LocalVersion);
            }
        }
    }
}