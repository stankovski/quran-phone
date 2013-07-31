using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Reflection;
using System.Windows;
using Microsoft.Phone.Tasks;

namespace QuranPhone.Utils
{
    /// <summary>
    ///     Error reporting based on http://blogs.msdn.com/b/andypennell/archive/2010/11/01/error-reporting-on-windows-phone-7.aspx?Redirected=true
    /// </summary>
    public class LittleWatson
    {
        private const string filename = "LittleWatson.txt";


        internal static void ReportException(Exception ex, string extra)
        {
            try
            {
                var nameHelper = new AssemblyName(Assembly.GetExecutingAssembly().FullName);

                using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    SafeDeleteFile(store);
                    using (TextWriter output = new StreamWriter(store.CreateFile(filename)))
                    {
                        output.WriteLine("QuranPhone Version: " + nameHelper.Version);
                        output.WriteLine("OS Version: " + Environment.OSVersion);
                        output.WriteLine(extra);
                        output.WriteLine(ex.Message);
                        output.WriteLine(ex.StackTrace);
                    }
                }
            }

            catch (Exception)
            {
            }
        }


        internal static void CheckForPreviousException()
        {
            try
            {
                string contents = null;

                using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (store.FileExists(filename))
                    {
                        using (
                            TextReader reader =
                                new StreamReader(store.OpenFile(filename, FileMode.Open, FileAccess.Read, FileShare.None))
                            )
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
                        var email = new EmailComposeTask();
                        email.To = "quran.phone@gmail.com";
                        email.Subject = "QuranPhone auto-generated problem report";
                        email.Body = contents;
                        SafeDeleteFile(IsolatedStorageFile.GetUserStoreForApplication());
                        email.Show();
                    }
                }
            }

            catch (Exception)
            {
            }
            finally
            {
                SafeDeleteFile(IsolatedStorageFile.GetUserStoreForApplication());
            }
        }


        private static void SafeDeleteFile(IsolatedStorageFile store)
        {
            try
            {
                store.DeleteFile(filename);
            }
            catch (Exception ex)
            {
            }
        }
    }
}