using System.Collections;
using System.Windows;
using System.Windows.Interactivity;
using QuranPhone.Common;
using QuranPhone.ViewModels;
using Telerik.Windows.Controls;

namespace QuranPhone.UI
{
    public class ListBoxPropertyBinder : Behavior<RadDataBoundListBox>
    {
        public static readonly DependencyProperty SelectedAyahProperty = DependencyProperty.Register("SelectedAyah",
            typeof (QuranAyah), typeof (ListBoxPropertyBinder), new PropertyMetadata(null, SelectedItemPropertyChanged));

        private RadDataBoundListBox listBox;

        static ListBoxPropertyBinder() {}

        public QuranAyah SelectedAyah
        {
            get { return GetValue(SelectedAyahProperty) as QuranAyah; }
            set { SetValue(SelectedAyahProperty, value); }
        }

        private void ChangeSelectedItem()
        {
            if (listBox == null)
            {
                return;
            }
            if (SelectedAyah == null)
            {
                listBox.SelectedItem = null;
            }
            else
            {
                object verse = getMatchedItem(SelectedAyah);
                if (verse != null)
                {
                    listBox.BringIntoView(verse);
                }
            }
        }

        private object getMatchedItem(QuranAyah ayah)
        {
            ICollection listboxItems = null;
            if (App.DetailsViewModel.CurrentPage == null ||
                (listBox.RealizedItems != null && listBox.RealizedItems.Length > 0))
            {
                listboxItems = listBox.RealizedItems;
            }
            else
            {
                listboxItems = App.DetailsViewModel.CurrentPage.Translations;
            }

            foreach (VerseViewModel item in listboxItems)
            {
                if (item.Surah == ayah.Sura && item.Ayah == ayah.Ayah)
                {
                    return item;
                }
            }
            return null;
        }

        private static void SelectedItemPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ((ListBoxPropertyBinder) sender).ChangeSelectedItem();
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            listBox = AssociatedObject;
        }
    }
}