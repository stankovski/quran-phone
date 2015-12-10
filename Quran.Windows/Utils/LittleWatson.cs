using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Reflection;
using System.Threading.Tasks;
using Quran.Core;
using Quran.Windows.NativeProvider;
using Windows.UI.Popups;

namespace Quran.Windows.Utils
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
                using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    SafeDeleteFile(store);
                    using (TextWriter output = new StreamWriter(store.CreateFile(filename)))
                    {
                        output.WriteLine("QuranPhone Version: " + SystemInfo.ApplicationVersion);
                        output.WriteLine("OS Version: " + SystemInfo.SystemVersion);
                        output.WriteLine(extra);
                        output.WriteLine(ex.ToString());
                    }
                }
            }
            catch (Exception)
            {
            }
        }


        internal static async Task CheckForPreviousException()
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
                    var dialog = new MessageDialog("A problem occurred the last time you ran this application. Would you like to send an email to report it?",
                            "Problem Report");
                    dialog.Commands.Add(new UICommand { Label = "OK", Id = 0 });
                    dialog.Commands.Add(new UICommand { Label = "Cancel", Id = 1 });
                    dialog.CancelCommandIndex = 1;
                    var result = await dialog.ShowAsync();
                    if ((int)result.Id == 0)
                    {
                        await QuranApp.NativeProvider.ComposeEmail("quran.phone@gmail.com", "QuranPhone auto-generated problem report");
                        SafeDeleteFile(IsolatedStorageFile.GetUserStoreForApplication());
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
            catch (Exception)
            {
            }
        }
    }
}