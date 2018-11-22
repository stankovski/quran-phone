using Quran.Core.Common;
using Quran.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quran.Core.Utils
{

    public static class AudioDownloadCacheUtils
    {
        //private static HashSet<BaseViewModel> list = new HashSet<BaseViewModel>();
        private static Dictionary<Tuple<ReciterItem, int>, AudioSurahViewModel> list = new Dictionary<Tuple<ReciterItem, int>, AudioSurahViewModel>();

        public static void AddToCache(AudioSurahViewModel viewModel)
        {
            list.Add(new Tuple<ReciterItem, int>(viewModel.Reciter, viewModel.Surah), viewModel);
        }

        public static void RemoveFromCache(AudioSurahViewModel viewModel)
        {
            list.Remove(new Tuple<ReciterItem, int>(viewModel.Reciter, viewModel.Surah));
        }

        public static AudioSurahViewModel GetSurahViewModel(ReciterItem reciter, int i)
        {
            AudioSurahViewModel vm;
            list.TryGetValue(new Tuple<ReciterItem, int>(reciter, i), out vm);

            return vm;
        }
    }
}
