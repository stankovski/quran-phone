using Windows.ApplicationModel.Resources; 

namespace Quran.Core.Properties 
{
    public class Resources 
    {
        private static readonly ResourceLoader resourceLoader; 

        static Resources() 
        {
            string currentAssemblyName;
            currentAssemblyName = typeof(Resources).AssemblyQualifiedName;
            string[] currentAssemblySplit;
            currentAssemblySplit = currentAssemblyName.Split(',');
            currentAssemblyName = currentAssemblySplit[1];
            try
            {
                resourceLoader = ResourceLoader.GetForCurrentView(currentAssemblyName + "/Resources");
            }
            catch
            {
                resourceLoader = ResourceLoader.GetForViewIndependentUse(currentAssemblyName + "/Resources");
            }
        }

        public static string juz2_description 
        {
            get { return resourceLoader.GetString("juz2_description"); }
        }

        public static string madani 
        {
            get { return resourceLoader.GetString("madani"); }
        }

        public static string makki 
        {
            get { return resourceLoader.GetString("makki"); }
        }

        public static string page_description 
        {
            get { return resourceLoader.GetString("page_description"); }
        }

        public static string quran_ayah 
        {
            get { return resourceLoader.GetString("quran_ayah"); }
        }

        public static string quran_juz2 
        {
            get { return resourceLoader.GetString("quran_juz2"); }
        }

        public static string quran_sura 
        {
            get { return resourceLoader.GetString("quran_sura"); }
        }

        public static string quran_sura_title 
        {
            get { return resourceLoader.GetString("quran_sura_title"); }
        }

        public static string ResourceFlowDirection 
        {
            get { return resourceLoader.GetString("ResourceFlowDirection"); }
        }

        public static string ResourceLanguage 
        {
            get { return resourceLoader.GetString("ResourceLanguage"); }
        }

        public static string sura_ayah_notification_str 
        {
            get { return resourceLoader.GetString("sura_ayah_notification_str"); }
        }

        public static string sura_names 
        {
            get { return resourceLoader.GetString("sura_names"); }
        }

        public static string app_name 
        {
            get { return resourceLoader.GetString("app_name"); }
        }

        public static string verse 
        {
            get { return resourceLoader.GetString("verse"); }
        }

        public static string verses 
        {
            get { return resourceLoader.GetString("verses"); }
        }

        public static string quarters 
        {
            get { return resourceLoader.GetString("quarters"); }
        }

        public static string translations 
        {
            get { return resourceLoader.GetString("translations"); }
        }

        public static string available_translations 
        {
            get { return resourceLoader.GetString("available_translations"); }
        }

        public static string downloaded_translations 
        {
            get { return resourceLoader.GetString("downloaded_translations"); }
        }

        public static string settings 
        {
            get { return resourceLoader.GetString("settings"); }
        }

        public static string loading_message 
        {
            get { return resourceLoader.GetString("loading_message"); }
        }

        public static string downloadPrompt 
        {
            get { return resourceLoader.GetString("downloadPrompt"); }
        }

        public static string downloadPrompt_title 
        {
            get { return resourceLoader.GetString("downloadPrompt_title"); }
        }

        public static string extracting_message 
        {
            get { return resourceLoader.GetString("extracting_message"); }
        }

        public static string quran_page 
        {
            get { return resourceLoader.GetString("quran_page"); }
        }

        public static string bookmarks 
        {
            get { return resourceLoader.GetString("bookmarks"); }
        }

        public static string bookmarks_ayah 
        {
            get { return resourceLoader.GetString("bookmarks_ayah"); }
        }

        public static string bookmarks_current_page 
        {
            get { return resourceLoader.GetString("bookmarks_current_page"); }
        }

        public static string bookmarks_page 
        {
            get { return resourceLoader.GetString("bookmarks_page"); }
        }

        public static string tags 
        {
            get { return resourceLoader.GetString("tags"); }
        }

        public static string delete_bookmark 
        {
            get { return resourceLoader.GetString("delete_bookmark"); }
        }

        public static string text_size 
        {
            get { return resourceLoader.GetString("text_size"); }
        }

        public static string about_us 
        {
            get { return resourceLoader.GetString("about_us"); }
        }

        public static string about_us_text 
        {
            get { return resourceLoader.GetString("about_us_text"); }
        }

        public static string prefs_ayah_before_translation_summary 
        {
            get { return resourceLoader.GetString("prefs_ayah_before_translation_summary"); }
        }

        public static string no_network_to_load 
        {
            get { return resourceLoader.GetString("no_network_to_load"); }
        }

        public static string download_cancel_confirmation 
        {
            get { return resourceLoader.GetString("download_cancel_confirmation"); }
        }

        public static string disable_sleep 
        {
            get { return resourceLoader.GetString("disable_sleep"); }
        }

        public static string please_restart 
        {
            get { return resourceLoader.GetString("please_restart"); }
        }

        public static string search 
        {
            get { return resourceLoader.GetString("search"); }
        }

        public static string search_results 
        {
            get { return resourceLoader.GetString("search_results"); }
        }

        public static string no_translation_to_search 
        {
            get { return resourceLoader.GetString("no_translation_to_search"); }
        }

        public static string keep_info_overlay 
        {
            get { return resourceLoader.GetString("keep_info_overlay"); }
        }

        public static string bookmark_ayah 
        {
            get { return resourceLoader.GetString("bookmark_ayah"); }
        }

        public static string quran_rub3 
        {
            get { return resourceLoader.GetString("quran_rub3"); }
        }

        public static string generate 
        {
            get { return resourceLoader.GetString("generate"); }
        }

        public static string generate_bookmarks_for_dua 
        {
            get { return resourceLoader.GetString("generate_bookmarks_for_dua"); }
        }

        public static string dua 
        {
            get { return resourceLoader.GetString("dua"); }
        }

        public static string night_mode 
        {
            get { return resourceLoader.GetString("night_mode"); }
        }

        public static string loading 
        {
            get { return resourceLoader.GetString("loading"); }
        }

        public static string change_language 
        {
            get { return resourceLoader.GetString("change_language"); }
        }

        public static string waiting 
        {
            get { return resourceLoader.GetString("waiting"); }
        }

        public static string waiting_for_power 
        {
            get { return resourceLoader.GetString("waiting_for_power"); }
        }

        public static string waiting_for_wifi 
        {
            get { return resourceLoader.GetString("waiting_for_wifi"); }
        }

        public static string bookmark 
        {
            get { return resourceLoader.GetString("bookmark"); }
        }

        public static string translation 
        {
            get { return resourceLoader.GetString("translation"); }
        }

        public static string contact_us 
        {
            get { return resourceLoader.GetString("contact_us"); }
        }

        public static string keep_orientation 
        {
            get { return resourceLoader.GetString("keep_orientation"); }
        }

        public static string auto_orientation 
        {
            get { return resourceLoader.GetString("auto_orientation"); }
        }

        public static string copy 
        {
            get { return resourceLoader.GetString("copy"); }
        }

        public static string reciters 
        {
            get { return resourceLoader.GetString("reciters"); }
        }

        public static string recite 
        {
            get { return resourceLoader.GetString("recite"); }
        }

        public static string loading_audio 
        {
            get { return resourceLoader.GetString("loading_audio"); }
        }

        public static string loading_data 
        {
            get { return resourceLoader.GetString("loading_data"); }
        }

        public static string loading_images 
        {
            get { return resourceLoader.GetString("loading_images"); }
        }

        public static string available_reciters 
        {
            get { return resourceLoader.GetString("available_reciters"); }
        }

        public static string downloaded_reciters 
        {
            get { return resourceLoader.GetString("downloaded_reciters"); }
        }

        public static string audio_download_blocks 
        {
            get { return resourceLoader.GetString("audio_download_blocks"); }
        }

        public static string audio 
        {
            get { return resourceLoader.GetString("audio"); }
        }

        public static string recite_ayah 
        {
            get { return resourceLoader.GetString("recite_ayah"); }
        }

        public static string pin_to_start 
        {
            get { return resourceLoader.GetString("pin_to_start"); }
        }

        public static string alternate_download_method 
        {
            get { return resourceLoader.GetString("alternate_download_method"); }
        }

        public static string none 
        {
            get { return resourceLoader.GetString("none"); }
        }

        public static string unlimited 
        {
            get { return resourceLoader.GetString("unlimited"); }
        }

        public static string number_of_ayah_to_repeat 
        {
            get { return resourceLoader.GetString("number_of_ayah_to_repeat"); }
        }

        public static string repeat_ayah 
        {
            get { return resourceLoader.GetString("repeat_ayah"); }
        }

        public static string times_to_repeat 
        {
            get { return resourceLoader.GetString("times_to_repeat"); }
        }

        public static string share_ayah 
        {
            get { return resourceLoader.GetString("share_ayah"); }
        }

        public static string home 
        {
            get { return resourceLoader.GetString("home"); }
        }

        public static string cancel 
        {
            get { return resourceLoader.GetString("cancel"); }
        }

        public static string select 
        {
            get { return resourceLoader.GetString("select"); }
        }

        public static string next 
        {
            get { return resourceLoader.GetString("next"); }
        }

        public static string pause 
        {
            get { return resourceLoader.GetString("pause"); }
        }

        public static string previous 
        {
            get { return resourceLoader.GetString("previous"); }
        }

        public static string go_to 
        {
            get { return resourceLoader.GetString("go_to"); }
        }

        public static string offline_audio 
        {
            get { return resourceLoader.GetString("offline_audio"); }
        }

        public static string quran_bookmarks 
        {
            get { return resourceLoader.GetString("quran_bookmarks"); }
        }

        public static string downloaded_audio 
        {
            get { return resourceLoader.GetString("downloaded_audio"); }
        }

        public static string streaming_audio 
        {
            get { return resourceLoader.GetString("streaming_audio"); }
        }

        public static string navigate 
        {
            get { return resourceLoader.GetString("navigate"); }
        }

        public static string page_number 
        {
            get { return resourceLoader.GetString("page_number"); }
        }

        public static string arabic_text_size 
        {
            get { return resourceLoader.GetString("arabic_text_size"); }
        }
    }

	public class LocalizedResources
    {

        public string juz2_description
        {
            get { return Resources.juz2_description; }
        }

        public string madani
        {
            get { return Resources.madani; }
        }

        public string makki
        {
            get { return Resources.makki; }
        }

        public string page_description
        {
            get { return Resources.page_description; }
        }

        public string quran_ayah
        {
            get { return Resources.quran_ayah; }
        }

        public string quran_juz2
        {
            get { return Resources.quran_juz2; }
        }

        public string quran_sura
        {
            get { return Resources.quran_sura; }
        }

        public string quran_sura_title
        {
            get { return Resources.quran_sura_title; }
        }

        public string ResourceFlowDirection
        {
            get { return Resources.ResourceFlowDirection; }
        }

        public string ResourceLanguage
        {
            get { return Resources.ResourceLanguage; }
        }

        public string sura_ayah_notification_str
        {
            get { return Resources.sura_ayah_notification_str; }
        }

        public string sura_names
        {
            get { return Resources.sura_names; }
        }

        public string app_name
        {
            get { return Resources.app_name; }
        }

        public string verse
        {
            get { return Resources.verse; }
        }

        public string verses
        {
            get { return Resources.verses; }
        }

        public string quarters
        {
            get { return Resources.quarters; }
        }

        public string translations
        {
            get { return Resources.translations; }
        }

        public string available_translations
        {
            get { return Resources.available_translations; }
        }

        public string downloaded_translations
        {
            get { return Resources.downloaded_translations; }
        }

        public string settings
        {
            get { return Resources.settings; }
        }

        public string loading_message
        {
            get { return Resources.loading_message; }
        }

        public string downloadPrompt
        {
            get { return Resources.downloadPrompt; }
        }

        public string downloadPrompt_title
        {
            get { return Resources.downloadPrompt_title; }
        }

        public string extracting_message
        {
            get { return Resources.extracting_message; }
        }

        public string quran_page
        {
            get { return Resources.quran_page; }
        }

        public string bookmarks
        {
            get { return Resources.bookmarks; }
        }

        public string bookmarks_ayah
        {
            get { return Resources.bookmarks_ayah; }
        }

        public string bookmarks_current_page
        {
            get { return Resources.bookmarks_current_page; }
        }

        public string bookmarks_page
        {
            get { return Resources.bookmarks_page; }
        }

        public string tags
        {
            get { return Resources.tags; }
        }

        public string delete_bookmark
        {
            get { return Resources.delete_bookmark; }
        }

        public string text_size
        {
            get { return Resources.text_size; }
        }

        public string about_us
        {
            get { return Resources.about_us; }
        }

        public string about_us_text
        {
            get { return Resources.about_us_text; }
        }

        public string prefs_ayah_before_translation_summary
        {
            get { return Resources.prefs_ayah_before_translation_summary; }
        }

        public string no_network_to_load
        {
            get { return Resources.no_network_to_load; }
        }

        public string download_cancel_confirmation
        {
            get { return Resources.download_cancel_confirmation; }
        }

        public string disable_sleep
        {
            get { return Resources.disable_sleep; }
        }

        public string please_restart
        {
            get { return Resources.please_restart; }
        }

        public string search
        {
            get { return Resources.search; }
        }

        public string search_results
        {
            get { return Resources.search_results; }
        }

        public string no_translation_to_search
        {
            get { return Resources.no_translation_to_search; }
        }

        public string keep_info_overlay
        {
            get { return Resources.keep_info_overlay; }
        }

        public string bookmark_ayah
        {
            get { return Resources.bookmark_ayah; }
        }

        public string quran_rub3
        {
            get { return Resources.quran_rub3; }
        }

        public string generate
        {
            get { return Resources.generate; }
        }

        public string generate_bookmarks_for_dua
        {
            get { return Resources.generate_bookmarks_for_dua; }
        }

        public string dua
        {
            get { return Resources.dua; }
        }

        public string night_mode
        {
            get { return Resources.night_mode; }
        }

        public string loading
        {
            get { return Resources.loading; }
        }

        public string change_language
        {
            get { return Resources.change_language; }
        }

        public string waiting
        {
            get { return Resources.waiting; }
        }

        public string waiting_for_power
        {
            get { return Resources.waiting_for_power; }
        }

        public string waiting_for_wifi
        {
            get { return Resources.waiting_for_wifi; }
        }

        public string bookmark
        {
            get { return Resources.bookmark; }
        }

        public string translation
        {
            get { return Resources.translation; }
        }

        public string contact_us
        {
            get { return Resources.contact_us; }
        }

        public string keep_orientation
        {
            get { return Resources.keep_orientation; }
        }

        public string auto_orientation
        {
            get { return Resources.auto_orientation; }
        }

        public string copy
        {
            get { return Resources.copy; }
        }

        public string reciters
        {
            get { return Resources.reciters; }
        }

        public string recite
        {
            get { return Resources.recite; }
        }

        public string loading_audio
        {
            get { return Resources.loading_audio; }
        }

        public string loading_data
        {
            get { return Resources.loading_data; }
        }

        public string loading_images
        {
            get { return Resources.loading_images; }
        }

        public string available_reciters
        {
            get { return Resources.available_reciters; }
        }

        public string downloaded_reciters
        {
            get { return Resources.downloaded_reciters; }
        }

        public string audio_download_blocks
        {
            get { return Resources.audio_download_blocks; }
        }

        public string audio
        {
            get { return Resources.audio; }
        }

        public string recite_ayah
        {
            get { return Resources.recite_ayah; }
        }

        public string pin_to_start
        {
            get { return Resources.pin_to_start; }
        }

        public string alternate_download_method
        {
            get { return Resources.alternate_download_method; }
        }

        public string none
        {
            get { return Resources.none; }
        }

        public string unlimited
        {
            get { return Resources.unlimited; }
        }

        public string number_of_ayah_to_repeat
        {
            get { return Resources.number_of_ayah_to_repeat; }
        }

        public string repeat_ayah
        {
            get { return Resources.repeat_ayah; }
        }

        public string times_to_repeat
        {
            get { return Resources.times_to_repeat; }
        }

        public string share_ayah
        {
            get { return Resources.share_ayah; }
        }

        public string home
        {
            get { return Resources.home; }
        }

        public string cancel
        {
            get { return Resources.cancel; }
        }

        public string select
        {
            get { return Resources.select; }
        }

        public string next
        {
            get { return Resources.next; }
        }

        public string pause
        {
            get { return Resources.pause; }
        }

        public string previous
        {
            get { return Resources.previous; }
        }

        public string go_to
        {
            get { return Resources.go_to; }
        }

        public string offline_audio
        {
            get { return Resources.offline_audio; }
        }

        public string quran_bookmarks
        {
            get { return Resources.quran_bookmarks; }
        }

        public string downloaded_audio
        {
            get { return Resources.downloaded_audio; }
        }

        public string streaming_audio
        {
            get { return Resources.streaming_audio; }
        }

        public string navigate
        {
            get { return Resources.navigate; }
        }

        public string page_number
        {
            get { return Resources.page_number; }
        }

        public string arabic_text_size
        {
            get { return Resources.arabic_text_size; }
        }
    }
}
