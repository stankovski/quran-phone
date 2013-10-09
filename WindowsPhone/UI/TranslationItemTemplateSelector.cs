using System.Windows;

namespace QuranPhone.UI
{
    public class TranslationItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ItemDownloadedTemplate { get; set; }
        public DataTemplate ItemAvailableTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var translationItem = item as ObservableTranslationItem;

            if (translationItem != null)
            {
                if (translationItem.Exists)
                {
                    return ItemDownloadedTemplate;
                }
                return ItemAvailableTemplate;
            }

            return base.SelectTemplate(item, container);
        }
    }
}