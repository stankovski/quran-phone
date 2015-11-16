using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml;

namespace Quran.WindowsPhone.UI
{
    public class Template
    {
        public string DataType { get; set; }

        public DataTemplate DataTemplate { get; set; }
    }

    public class TemplateCollection : Collection<Template>
    {
    }


    public class CustomDataTemplateSelector : DataTemplateSelector
    {
        public TemplateCollection Templates { get; set; }

        private IList<Template> _templateCache { get; set; }

        public CustomDataTemplateSelector()
        {
        }

        private void InitTemplateCollection()
        {
            _templateCache = Templates.ToList();
        }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (_templateCache == null)
            {
                InitTemplateCollection();
            }

            if (item != null)
            {
                var dataType = item.GetType().ToString();

                var match = _templateCache.Where(m => m.DataType == dataType).FirstOrDefault();

                if (match != null)
                {
                    return match.DataTemplate;
                }
            }

            return base.SelectTemplate(item, container);
        }
    }
}
