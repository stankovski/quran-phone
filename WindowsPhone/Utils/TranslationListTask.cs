using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json.Linq;
using QuranPhone.Common;
using QuranPhone.Data;
using QuranPhone.Resources;

namespace QuranPhone.Utils
{
    public static class TranslationListTask
    {
        public const string WebServiceUrl = "http://android.quran.com/data/translations.php?v=2";
        private const string CachedResponseFileName = "cached-translation-list";

        private static void CacheResponse(string response)
        {
            try
            {
                using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    string filePath = GetCachedResponseFilePath();
                    if (isf.FileExists(filePath))
                    {
                        isf.DeleteFile(filePath);
                    }
                    QuranFileUtils.MakeDirectory(Path.GetDirectoryName(filePath));
                    using (
                        IsolatedStorageFileStream stream = isf.OpenFile(filePath, FileMode.OpenOrCreate,
                            FileAccess.ReadWrite))
                    {
                        using (var writer = new StreamWriter(stream))
                        {
                            writer.Write(response);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("failed to cache response: " + e.Message);
            }
        }

        private static string LoadCachedResponse()
        {
            string response = null;
            try
            {
                using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    string filePath = GetCachedResponseFilePath();
                    if (!isf.FileExists(filePath))
                    {
                        return null;
                    }
                    using (
                        IsolatedStorageFileStream stream = isf.OpenFile(filePath, FileMode.Open, FileAccess.ReadWrite))
                    {
                        using (var reader = new StreamReader(stream))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("failed reading cached response: " + e.Message);
            }
            return response;
        }

        public static async Task<IEnumerable<TranslationItem>> DownloadTranslations(bool useCache, string tag)
        {
            bool shouldUseCache = false;
            if (useCache)
            {
                var when = SettingsUtils.Get<DateTime>(Constants.PrefLastUpdatedTranslations);
                DateTime now = DateTime.Now;
                if (when.AddMilliseconds(Constants.MinimumTranslationRefreshTime) > now)
                {
                    shouldUseCache = true;
                }
            }

            string text = null;
            if (shouldUseCache)
            {
                text = LoadCachedResponse();
            }

            bool refreshed = false;
            if (string.IsNullOrEmpty(text))
            {
                var request = (HttpWebRequest) WebRequest.Create(WebServiceUrl);
                request.Method = HttpMethod.Get;
                HttpWebResponse response = await request.GetResponseAsync();
                using (var sr = new StreamReader(response.GetResponseStream()))
                {
                    text = sr.ReadToEnd();
                }

                if (string.IsNullOrEmpty(text))
                {
                    text = LoadCachedResponse();
                }

                if (string.IsNullOrEmpty(text))
                {
                    MessageBox.Show(AppResources.no_network_to_load);
                    return null;
                }

                if (useCache)
                {
                    CacheResponse(text);
                }
                refreshed = true;
            }

            var adapter = new TranslationsDBAdapter();
            List<TranslationItem> cachedItems = adapter.GetTranslations();
            var items = new List<TranslationItem>();
            var updates = new List<TranslationItem>();

            try
            {
                JObject token = JObject.Parse(text);
                foreach (JToken t in token["data"])
                {
                    var item = new TranslationItem();
                    item.Id = t["id"].ToObject<int>();
                    item.Name = (string) t["displayName"];
                    item.Translator = (string) t["translator_foreign"];
                    if (string.IsNullOrEmpty(item.Translator))
                    {
                        item.Translator = (string) t["translator"];
                    }
                    item.LatestVersion = t["current_version"].ToObject<int>();
                    item.Filename = (string) t["fileName"];
                    item.Url = (string) t["fileUrl"];
                    if (item.Url.EndsWith("ext=zip", StringComparison.InvariantCultureIgnoreCase))
                    {
                        item.Compressed = true;
                    }


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
                                using (var mHandler = new DatabaseHandler<QuranAyah>(item.Filename))
                                {
                                    if (mHandler.ValidDatabase())
                                    {
                                        item.LocalVersion = mHandler.GetTextVersion();
                                    }
                                }
                            }
                            catch
                            {
                                MessageBox.Show("exception opening database: " + item.Filename);
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
                    SettingsUtils.Set(Constants.PrefLastUpdatedTranslations, DateTime.Now);
                }

                if (updates.Count() > 0)
                {
                    adapter.WriteTranslationUpdates(updates);
                }
            }
            catch (Exception je)
            {
                MessageBox.Show("error parsing json: " + je.Message);
            }

            return items;
        }

        private static string GetCachedResponseFilePath()
        {
            return Path.Combine(QuranFileUtils.GetQuranDatabaseDirectory(false, true), CachedResponseFileName);
        }
    }
}