using System.Windows;
using System.Windows.Interactivity;
using Telerik.Windows.Controls;

namespace QuranPhone.UI
{
    public class ListBoxPropertyBinder : Behavior<RadDataBoundListBox>
    {
        static ListBoxPropertyBinder()
        {
        }

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof(object), typeof(ListBoxPropertyBinder), new PropertyMetadata(null, SelectedItemPropertyChanged));

        private RadDataBoundListBox listBox;

        public object SelectedItem
        {
            get
            {
                return this.GetValue(ListBoxPropertyBinder.SelectedItemProperty);
            }
            set
            {
                this.SetValue(ListBoxPropertyBinder.SelectedItemProperty, value);
            }
        }

        private void ChangeSelectedItem()
        {
            if (this.listBox == null)
                return;
            this.listBox.SelectedItem = this.SelectedItem;
            this.listBox.BringIntoView(this.SelectedItem);
        }

        private static void SelectedItemPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ((ListBoxPropertyBinder)sender).ChangeSelectedItem();
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            this.listBox = this.AssociatedObject;
            if (this.listBox == null)
                return;
            this.listBox.SelectedItem = this.SelectedItem;
        }
    }
}
