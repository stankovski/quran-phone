using System.Windows;
using Newtonsoft.Json.Linq;
using QuranPhone.Common;
using QuranPhone.Data;
using QuranPhone.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace QuranPhone.Utils
{
    public class TranslationListTask
    {
        public const string WEB_SERVICE_URL = "http://android.quran.com/data/translations.php?v=2";
        private const string CACHED_RESPONSE_FILE_NAME = "cached-translation-list";

        private static void cacheResponse(string response)
        {
            try
            {
                using (var isf = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    var filePath = getCachedResponseFilePath();
                    if (isf.FileExists(filePath))
                        isf.DeleteFile(filePath);
                    QuranFileUtils.MakeDirectory(Path.GetDirectoryName(filePath));
                    using (var stream = isf.OpenFile(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                    using (var writer = new StreamWriter(stream))
                    {
                        writer.Write(response);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("failed to cache response: " + e.Message);
            }
        }

        private static string loadCachedResponse()
        {
            string response = null;
            try
            {
                using (var isf = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    var filePath = getCachedResponseFilePath();
                    if (!isf.FileExists(filePath))
                        return null;
                    using (var stream = isf.OpenFile(filePath, FileMode.Open, FileAccess.ReadWrite))
                    using (var reader = new StreamReader(stream))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("failed reading cached response: " + e.Message);
            }
            return response;
        }

        public static async Task<IEnumerable<TranslationItem>> DownloadTranslations(bool useCache, string tag)
        {
            bool shouldUseCache = false;
            if (useCache)
            {
                var when = SettingsUtils.Get<DateTime>(Constants.PREF_LAST_UPDATED_TRANSLATIONS);
                var now = DateTime.Now;
                if (when.AddMilliseconds(Constants.MIN_TRANSLATION_REFRESH_TIME) > now)
                {
                    shouldUseCache = true;
                }
            }

            string text = null;
            if (shouldUseCache)
            {
                text = loadCachedResponse();
            }

            bool refreshed = false;
            if (string.IsNullOrEmpty(text))
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(WEB_SERVICE_URL);
                request.Method = HttpMethod.Get;
                HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();
                using (var sr = new StreamReader(response.GetResponseStream()))
                {
                    text = sr.ReadToEnd();
                }

                if (string.IsNullOrEmpty(text))
                {
                    text = loadCachedResponse();
                }

                if (string.IsNullOrEmpty(text))
                {
                    MessageBox.Show(Resources.AppResources.no_network_to_load);
                    return null;
                }

                if (useCache)
                {
                    cacheResponse(text);
                }
                refreshed = true;
            }

            TranslationsDBAdapter adapter = new TranslationsDBAdapter();
            var cachedItems = adapter.GetTranslations();
            List<TranslationItem> items = new List<TranslationItem>();
            List<TranslationItem> updates = new List<TranslationItem>();
            
            try
            {
                var token = JObject.Parse(text);
                foreach(var t in token["data"]) 
                {
                    TranslationItem item = new TranslationItem();
                    item.Id = t["id"].ToObject<int>();
                    item.Name = (string)t["displayName"];
                    item.Translator = (string)t["translator_foreign"];
                    if (string.IsNullOrEmpty(item.Translator))
                        item.Translator = (string)t["translator"];
                    item.LatestVersion = t["current_version"].ToObject<int>();
                    item.Filename = (string)t["fileName"];
                    item.Url = (string)t["fileUrl"];
                    if (item.Url.EndsWith("ext=zip", StringComparison.InvariantCultureIgnoreCase))
                        item.Compressed = true;
                

                    int firstParen = item.Name.IndexOf("(");
                    if (firstParen != -1)
                    {
                        item.Name = item.Name.Substring(0, firstParen - 1);
                    }

                    string databaseDir = QuranFileUtils.GetQuranDatabaseDirectory(false);
                    item.Exists = QuranFileUtils.FileExists(Path.Combine(databaseDir, item.Filename));

                    bool needsUpdate = false;
                    TranslationItem localItem = cachedItems.Where(ti => ti.Id == item.Id).FirstOrDefault();
                    if (item.Exists)
                    {
                        if (localItem != null)
                        {
                            item.LocalVersion = localItem.LocalVersion;
                        }
                        else if (item.LatestVersion > -1)
                        {
                            needsUpdate = true;
                            try
                            {
                                using (DatabaseHandler mHandler = new DatabaseHandler(item.Filename))
                                {
                                    if (mHandler.ValidDatabase())
                                    {
                                        item.LocalVersion = mHandler.GetTextVersion();
                                    }
                                }
                            }
                            catch
                            {
                                Debug.WriteLine("exception opening database: " + item.Filename);
                            }
                        }
                        else 
                        { 
                            needsUpdate = true; 
                        }
                    }
                    else if (localItem != null)
                    {
                        needsUpdate = true;
                    }

                    if (needsUpdate)
                    {
                        updates.Add(item);
                    }

                    items.Add(item);
                }

                if (refreshed)
                {
                    SettingsUtils.Set<DateTime>(Constants.PREF_LAST_UPDATED_TRANSLATIONS, DateTime.Now);
                }

                if (updates.Count() > 0)
                {
                    adapter.WriteTranslationUpdates(updates);
                }
            }
            catch (Exception je)
            {
                Debug.WriteLine("error parsing json: " + je.Message);
            }

            return items;
        }

        private static string getCachedResponseFilePath()
        {
            string fileName = CACHED_RESPONSE_FILE_NAME;
            string dir = QuranFileUtils.GetQuranDatabaseDirectory(false, true);
            return Path.Combine(dir, fileName);
        }
    }
}
