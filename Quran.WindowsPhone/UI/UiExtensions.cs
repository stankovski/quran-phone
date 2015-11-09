using Windows.UI.Xaml;

namespace QuranPhone.UI
{
    public static class UiExtensions
    {
        public static object TryFindResource(this FrameworkElement element, object resourceKey)
        {
            var currentElement = element;

            while (currentElement != null)
            {
                var resource = currentElement.Resources[resourceKey];
                if (resource != null)
                {
                    return resource;
                }

                currentElement = currentElement.Parent as FrameworkElement;
            }

            return Application.Current.Resources[resourceKey];
        }

        public static object TryFindResource(this Application application, object resourceKey)
        {
            return Application.Current.Resources[resourceKey];
        }
    }
}
