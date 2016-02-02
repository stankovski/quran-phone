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
using Microsoft.ApplicationInsights;
using Newtonsoft.Json;
using System.Net.Http;

namespace Quran.Core.Utils
{
    public class TranslationListTask
    {
        public const string WEB_SERVICE_URL = "http://android.quran.com/data/translations.php?v=2";
        private const string CACHED_RESPONSE_FILE_NAME = "cached-translation-list";
        private static TelemetryClient telemetry = new TelemetryClient();

        private static async Task CacheResponse(string response)
        {
            try
            {
                var filePath = await GetCachedResponseFilePath();
                await FileUtils.WriteFile(filePath, response);
            }
            catch (Exception e)
            {
                Debug.WriteLine("failed to cache response: " + e.Message);
                telemetry.TrackException(e, new Dictionary<string, string> { { "Scenario", "CacheTranslationList" } });
            }
        }

        private static async Task<string> LoadCachedResponse()
        {
            var filePath = await GetCachedResponseFilePath();
            return await FileUtils.ReadFile(filePath);
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
                text = await LoadCachedResponse();
            }

            bool refreshed = false;
            if (string.IsNullOrEmpty(text))
            {
                HttpClient httpClient = new HttpClient();
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, WEB_SERVICE_URL);
                var response = await httpClient.SendAsync(request);
                text = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrEmpty(text))
                {
                    text = await LoadCachedResponse();
                }

                if (string.IsNullOrEmpty(text))
                {
                    return null;
                }

                if (useCache)
                {
                    await CacheResponse(text);
                }
                refreshed = true;
            }

            List<TranslationItem> items = new List<TranslationItem>();
            
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

                    item.Exists = await FileUtils.FileExists(FileUtils.DatabaseFolder, item.Filename);

                    items.Add(item);
                }

                if (refreshed)
                {
                    SettingsUtils.Set<DateTime>(Constants.PREF_LAST_UPDATED_TRANSLATIONS, DateTime.Now);
                }
            }
            catch (JsonException je)
            {
                telemetry.TrackException(je, new Dictionary<string, string> { { "Scenario", "ParsingDownloadedTranslationJson" } });
            }

            return items;
        }

        private static async Task<string> GetCachedResponseFilePath()
        {
            string fileName = CACHED_RESPONSE_FILE_NAME;
            string dir = FileUtils.GetQuranDatabaseDirectory();
            return Path.Combine(dir, fileName);
        }
    }
}
