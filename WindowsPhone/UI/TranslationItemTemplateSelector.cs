using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace QuranPhone.UI
{
    public class TranslationItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ItemDownloadedTemplate { get; set; }
        public DataTemplate ItemAvailableTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            ObservableTranslationItem translationItem = item as ObservableTranslationItem;

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
