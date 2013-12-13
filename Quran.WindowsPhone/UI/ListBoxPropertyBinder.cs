using System.Collections;
using System.Windows;
using System.Windows.Interactivity;
using Quran.Core;
using Quran.Core.Common;
using Quran.Core.ViewModels;
using Telerik.Windows.Controls;

namespace Quran.WindowsPhone.UI
{
    public class ListBoxPropertyBinder : Behavior<RadDataBoundListBox>
    {
        static ListBoxPropertyBinder()
        {
        }

        public static readonly DependencyProperty SelectedAyahProperty = DependencyProperty.Register("SelectedAyah", typeof(QuranAyah), typeof(ListBoxPropertyBinder), new PropertyMetadata(null, SelectedItemPropertyChanged));

        private RadDataBoundListBox listBox;

        public QuranAyah SelectedAyah
        {
            get
            {
                return this.GetValue(ListBoxPropertyBinder.SelectedAyahProperty) as QuranAyah;
            }
            set
            {
                this.SetValue(ListBoxPropertyBinder.SelectedAyahProperty, value);
            }
        }

        private void ChangeSelectedItem()
        {
            if (this.listBox == null)
                return;
            if (SelectedAyah == null)
            {
                this.listBox.SelectedItem = null;
            }
            else
            {
                try
                {
                    var verse = getMatchedItem(this.SelectedAyah);
                    if (verse != null)
                    {
                        this.listBox.BringIntoView(verse);
                    }
                }
                catch
                {
                    // Ignore exception
                }
            }
        }

        private object getMatchedItem(QuranAyah ayah)
        {
            ICollection listboxItems = null;
            if (QuranApp.DetailsViewModel.CurrentPage == null || 
                (this.listBox.RealizedItems != null && this.listBox.RealizedItems.Length > 0))
                listboxItems = this.listBox.RealizedItems;
            else
                listboxItems = QuranApp.DetailsViewModel.CurrentPage.Translations;

            foreach (VerseViewModel item in listboxItems)
            {
                if (item.Surah == ayah.Surah && item.Ayah == ayah.Ayah)
                {
                    return item;
                }
            }
            return null;
        }

        private static void SelectedItemPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ((ListBoxPropertyBinder)sender).ChangeSelectedItem();
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            this.listBox = this.AssociatedObject;
        }
    }
}
