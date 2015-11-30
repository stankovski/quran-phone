using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Quran.Core.ViewModels
{
    public class NavigationLink
    {
        public Symbol Symbol { get; set; }
        public string Label { get; set; }
        public Action Action { get; set; }
    }
}
