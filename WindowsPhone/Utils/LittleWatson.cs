using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Reflection;
using System.Windows;
using Microsoft.Phone.Tasks;

namespace QuranPhone.Utils
{
    public static class LittleWatson
    {
        private const string Filename = "LittleWatson.txt";

        public static void ReportException(Exception ex, string extra)
        {
            var nameHelper = new AssemblyName(Assembly.GetExecutingAssembly().FullName);

            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                SafeDeleteFile(store);
                using (TextWriter output = new StreamWriter(store.CreateFile(Filename)))
                {
                    output.WriteLine("App Version: " + nameHelper.Version);
                    output.WriteLine("OS Version: " + Environment.OSVersion);
                    output.WriteLine(extra);
                    output.WriteLine(ex.Message);
                    output.WriteLine(ex.StackTrace);
                }
            }
        }

        public static void CheckForPreviousException()
        {
            string contents = null;

            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (store.FileExists(Filename))
                {
                    using (TextReader reader = new StreamReader(store.OpenFile(Filename, FileMode.Open, FileAccess.Read, FileShare.None)))
                    {
                        contents = reader.ReadToEnd();
                    }

                    SafeDeleteFile(store);
                }
            }

            if (contents != null)
            {
                if (
                    MessageBox.Show(
                        "A problem occurred the last time you ran this application. Would you like to send an email to report it?",
                        "Problem Report", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    //new EmailComposeTask
                    //{
                    //    To = "quran.phone@gmail.com",
                    //    Subject = "QuranPhone auto-generated problem report",
                    //    Body = contents
                    //}.Show();
                }
            }
        }

        private static void SafeDeleteFile(IsolatedStorageFile store)
        {
            try
            {
                store.DeleteFile(Filename);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}