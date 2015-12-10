using System.Windows;
using Quran.Core.ViewModels;
using Windows.UI.Xaml;

namespace Quran.Windows.UI
{
    public class RecitationItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ItemDownloadedTemplate { get; set; }
        public DataTemplate ItemAvailableTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            ObservableReciterItem translationItem = item as ObservableReciterItem;

            if (translationItem != null)
            {
                if (translationItem.Exists)
                    return ItemDownloadedTemplate;
                else
                    return ItemAvailableTemplate;
            }

            return base.SelectTemplate(item, container);
        }
    }
}
