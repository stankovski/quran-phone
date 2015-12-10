using Windows.ApplicationModel.Resources; 

namespace Quran.Core.Properties 
{
    public class Resources 
    {
        private static readonly ResourceLoader resourceLoader; 

        static Resources() 
        {
            
            string executingAssemblyName;
            executingAssemblyName = Windows.UI.Xaml.Application.Current.GetType().AssemblyQualifiedName;
            string[] executingAssemblySplit;
            executingAssemblySplit = executingAssemblyName.Split(',');
            executingAssemblyName = executingAssemblySplit[1];
            string currentAssemblyName;
            currentAssemblyName = typeof(Resources).AssemblyQualifiedName;
            string[] currentAssemblySplit;
            currentAssemblySplit = currentAssemblyName.Split(',');
            currentAssemblyName = currentAssemblySplit[1];
            if (executingAssemblyName.Equals(currentAssemblyName))
            {
                resourceLoader = ResourceLoader.GetForCurrentView("Resources");
            }
            else
            {
                resourceLoader = ResourceLoader.GetForCurrentView(currentAssemblyName + "/Resources");
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

        public static string quran_bookmarks_lower 
        {
            get { return resourceLoader.GetString("quran_bookmarks_lower"); }
        }

        public static string quran_juz2_lower 
        {
            get { return resourceLoader.GetString("quran_juz2_lower"); }
        }

        public static string quran_sura_lower 
        {
            get { return resourceLoader.GetString("quran_sura_lower"); }
        }

        public static string quran_tags_lower 
        {
            get { return resourceLoader.GetString("quran_tags_lower"); }
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
    }
}
