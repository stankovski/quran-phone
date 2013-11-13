using Cirrious.CrossCore;
using Cirrious.MvvmCross.Plugins.File;
using Newtonsoft.Json.Linq;
using Quran.Core.Common;
using Quran.Core.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Quran.Core.Utils
{
    public class TranslationListTask
    {
        public const string WEB_SERVICE_URL = "http://android.quran.com/data/translations.php?v=2";
        private const string CACHED_RESPONSE_FILE_NAME = "cached-translation-list";

        private static IMvxFileStore FileStore
        {
            get { return Mvx.Resolve<IMvxFileStore>(); }
        }

        private static void cacheResponse(string response)
        {
            try
            {
                var filePath = getCachedResponseFilePath();
                if (FileStore.Exists(filePath))
                    FileStore.DeleteFile(filePath);
                FileUtils.MakeDirectory(PathHelper.GetDirectoryName(filePath));
                FileStore.WriteFile(filePath, response);
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
                var filePath = getCachedResponseFilePath();
                if (!FileStore.Exists(filePath))
                    return null;
                string content = null;
                if (FileStore.TryReadTextFile(filePath, out content))
                    return content;
                else
                    return null;
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
                var response = await request.GetResponseAsync();
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
                    return null;
                }

                if (useCache)
                {
                    cacheResponse(text);
                }
                refreshed = true;
            }

            TranslationsDatabaseHandler adapter = new TranslationsDatabaseHandler();
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
                    if (item.Url.EndsWith("ext=zip", StringComparison.OrdinalIgnoreCase))
                        item.Compressed = true;
                

                    int firstParen = item.Name.IndexOf("(");
                    if (firstParen != -1)
                    {
                        item.Name = item.Name.Substring(0, firstParen - 1);
                    }

                    string databaseDir = FileUtils.GetQuranDatabaseDirectory(false);
                    item.Exists = FileUtils.FileExists(FileUtils.Combine(databaseDir, item.Filename));

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
                                using (var mHandler = new QuranDatabaseHandler<QuranAyah>(item.Filename))
                                {
                                    item.LocalVersion = mHandler.GetTextVersion();
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
            string dir = FileUtils.GetQuranDatabaseDirectory(false, true);
            return FileUtils.Combine(dir, fileName);
        }
    }
}
