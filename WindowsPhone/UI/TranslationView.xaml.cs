using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using QuranPhone.Utils;
using QuranPhone.ViewModels;

namespace QuranPhone.UI
{
    public partial class TranslationView : UserControl, IDisposable
    {
        public TranslationView()
        {
            InitializeComponent();
        }


        private void UserControl_MouseEnter_1(object sender, System.Windows.Input.MouseEventArgs e)
        {
            memoryUsage.Text = PhoneUtils.CurrentMemoryUsage();
        }

        public void Dispose()
        {
            if (DataContext != null)
            {
                ((PageViewModel)DataContext).Verses.Clear();
                DataContext = null;
            }
        }
    }
}
