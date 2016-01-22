// --------------------------------------------------------------------------------------------------------------------
// <summary>
//    Defines the TranslationslistViewModel type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Quran.Core.Common;
using Quran.Core.Data;
using Quran.Core.Properties;
using Quran.Core.Utils;
using Windows.Storage;
using Windows.Storage.Search;

namespace Quran.Core.ViewModels
{
    public class AudioSurahViewModel : DownloadableViewModelBase
    {
        public AudioSurahViewModel()
        {

        }

        public AudioSurahViewModel(ReciterItem reciter, int surah)
        {
            Reciter = reciter;
            Surah = surah;
        }

        public async Task Delete()
        {
            var baseDirectory = await Reciter.GetStorageFolder();

            var fileQuery = baseDirectory.CreateFileQueryWithOptions(new QueryOptions
            {
                UserSearchFilter = string.Format(CultureInfo.InvariantCulture,
                    "{0:000}*.mp3", Surah),
                FolderDepth = FolderDepth.Shallow
            });

            var files = await fileQuery.GetFilesAsync();
            foreach (var file in files)
            {
                await file.DeleteAsync();
            }
            await Refresh();
        }

        public async Task Download()
        {
            // checking if need to download mp3
            var missingFiles = await AudioUtils.GetMissingFiles(new QuranAudioTrack
            {
                Surah = Surah,
                Ayah = 1,
                ReciterId = Reciter.Id
            });

            if (missingFiles.Count > 0)
            {
                string url = Reciter.ServerUrl;
                StorageFolder destination = await Reciter.GetStorageFolder();
                await DownloadMultipleViaHttpClient(
                    missingFiles.Select(f => Path.Combine(url, f)).ToArray(),
                    destination, Resources.loading_audio);
            }

            await Refresh();
        }

        public override async Task Initialize()
        {
            await base.Initialize();
            await Refresh();
        }

        public override async Task Refresh()
        {
            var missingFiles = await AudioUtils.GetMissingFiles(new QuranAudioTrack
            {
                Surah = Surah,
                Ayah = 1,
                ReciterId = Reciter.Id
            });
            if (missingFiles.Count == 0 ||
                (missingFiles.Count == 1 && missingFiles.Contains("001001.mp3")))
            {
                Exists = true;
            }
            else
            {
                Exists = false;
            }
            await base.Refresh();
        }

        public ReciterItem Reciter { get; set; }

        public int Surah { get; set; }

        public string SurahName { get; set; }

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
    }

    /// <summary>
    /// Define the SurahDownloadViewModel type.
    /// </summary>
    public class SurahDownloadViewModel : BaseViewModel
    {
        public SurahDownloadViewModel()
        {
            Surahs = new ObservableCollection<AudioSurahViewModel>();
        }

        public ReciterItem Reciter { get; set; }

        public ObservableCollection<AudioSurahViewModel> Surahs { get; private set; }

        public override Task Initialize()
        {
            for (int i = 1; i < Constants.SURAS_COUNT; i++)
            {
                Surahs.Add(new AudioSurahViewModel(Reciter, i)
                {
                    SurahName = QuranUtils.GetSurahName(i, true)
                });
            }
            return Refresh();
        }

        public override async Task Refresh()
        {
            this.IsLoading = true;
            foreach (var s in Surahs)
            {
                await s.Initialize();
            }
            this.IsLoading = false;
        }
    }
}
