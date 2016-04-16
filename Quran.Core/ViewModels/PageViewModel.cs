// --------------------------------------------------------------------------------------------------------------------
// <summary>
//    Defines the PageViewModel type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Quran.Core.Common;
using Quran.Core.Data;
using Quran.Core.Utils;
using Windows.UI.Xaml;

namespace Quran.Core.ViewModels
{
    /// <summary>
    /// Define the PageViewModel type.
    /// </summary>
    public class PageViewModel : BaseViewModel
    {
        public PageViewModel()
        {
            Translations = new ObservableCollection<VerseViewModel>();
        }

        public PageViewModel(int page, DetailsViewModel parent)
            : this()
        {
            PageNumber = page;
            Parent = parent;
        }

        #region Properties

        public DetailsViewModel Parent { get; set; }

        public ObservableCollection<VerseViewModel> Translations { get; set; }
        
        private int pageNumber;
        public int PageNumber
        {
            get { return pageNumber; }
            set
            {
                if (value == pageNumber)
                    return;

                pageNumber = value;

                base.OnPropertyChanged(() => PageNumber);
            }
        }

        private bool showTranslation;
        public bool ShowTranslation
        {
            get { return showTranslation; }
            set
            {
                if (value == showTranslation)
                    return;

                showTranslation = value;

                base.OnPropertyChanged(() => ShowTranslation);
            }
        }

        private Uri imageSource;
        public Uri ImageSource
        {
            get { return imageSource; }
            set
            {
                if (value == imageSource)
                    return;

                imageSource = value;

                base.OnPropertyChanged(() => ImageSource);
            }
        }

        public double ScreenWidth
        {
            get { return FileUtils.ScreenInfo.Width - 20; }
        }

        // QuranUtils Properties
        public String SuraName
        {
            get { return QuranUtils.GetSurahNameFromPage(PageNumber); }
        }

        public String JuzName
        {
            get { return QuranUtils.GetJuzString(PageNumber); }
        }

        #endregion Properties

        private bool _loadingTranslations = false;
        public async Task Load(string translationFile, string bismillahTranslation)
        {
            // Set image
            if (this.ImageSource == null)
            {
                this.ImageSource = FileUtils.GetImageOnlineUri(FileUtils.GetPageFileName(this.PageNumber));
            }

            // Skip if no translation file
            if (!await Parent.HasTranslationFile() || _loadingTranslations)
            {
                return;
            }

            // Set translation
            if (this.Translations.Count == 0)
            {
                try
                {
                    _loadingTranslations = true;
                    List<QuranAyah> versesTranslation = null;
                    using (var db = new QuranDatabaseHandler<QuranAyah>(translationFile))
                    {
                        versesTranslation = await new TaskFactory().StartNew(() => db.GetVerses(this.PageNumber));
                    }

                    FlowDirection flowDirection = FlowDirection.LeftToRight;
                    Regex translationFilePattern = new Regex(@"quran\.([\w-]+)\..*");
                    var fileMatch = translationFilePattern.Match(translationFile);
                    if (fileMatch.Success)
                    {
                        try
                        {
                            var cultureName = fileMatch.Groups[1].Value;
                            if (cultureName.Length > 2)
                            {
                                cultureName = cultureName.Substring(0, 2);
                            }
                            var cultureInfo = new CultureInfo(cultureName);
                            if (cultureInfo.TextInfo.IsRightToLeft)
                            {
                                flowDirection = FlowDirection.RightToLeft;
                            }
                        }
                        catch (Exception e)
                        {
                            telemetry.TrackException(e, new Dictionary<string, string> { { "Scenario", "ParseTranslationFileForFlowDirection" } });
                        }
                    }

                    List<ArabicAyah> versesArabic = null;
                    if (SettingsUtils.Get<bool>(Constants.PREF_SHOW_ARABIC_IN_TRANSLATION))
                    {
                        try
                        {
                            using (var dbArabic = new QuranDatabaseHandler<ArabicAyah>(FileUtils.ArabicDatabase))
                            {
                                versesArabic = await new TaskFactory().StartNew(() => dbArabic.GetVerses(this.PageNumber));
                            }
                        }
                        catch (Exception e)
                        {
                            telemetry.TrackException(e, new Dictionary<string, string> { { "Scenario", "OpenArabicDatabase" } });
                        }
                    }

                    int tempSurah = -1;
                    for (int i = 0; i < versesTranslation.Count; i++)
                    {
                        var verse = versesTranslation[i];
                        if (verse.Surah != tempSurah)
                        {
                            this.Translations.Add(new VerseViewModel(this.Parent)
                            {
                                IsHeader = true,
                                Text = QuranUtils.GetSurahName(verse.Surah, true)
                            });

                            tempSurah = verse.Surah;
                        }


                        string translation = verse.Text;
                        if (QuranUtils.HasBismillah(verse.Surah) && verse.Surah != 1 && verse.Ayah == 1)
                        {
                            translation = bismillahTranslation + " " + translation;
                        }
                        var verseViewModel = new VerseViewModel(this.Parent)
                        {
                            Surah = verse.Surah,
                            Ayah = verse.Ayah,
                            Text = translation,
                            FlowDirection = flowDirection
                        };
                        if (versesArabic != null)
                        {
                            verseViewModel.ArabicText = versesArabic[i].Text;
                        }

                        this.Translations.Add(verseViewModel);
                    }
                }
                catch (Exception e)
                {
                    // Try delete bad translation file if error is "no such table: verses"
                    try
                    {
                        if (e.Message.StartsWith("no such table:", StringComparison.OrdinalIgnoreCase))
                        {
                            await FileUtils.SafeFileDelete(Path.Combine(FileUtils.GetQuranDatabaseDirectory(),
                                                                   translationFile));
                        }
                    }
                    catch
                    {
                        // Do nothing
                    }
                    this.Translations.Add(new VerseViewModel(this.Parent) { Text = "Error loading translation..." });
                    telemetry.TrackException(e, new Dictionary<string, string> { { "Scenario", "LoadingTranslation" } });
                }
                finally
                {
                    _loadingTranslations = false;
                }
            }
        }
        public override Task Initialize()
        {
            return Refresh();
        }

        public override Task Refresh()
        {
            return Task.FromResult(0);
        }

        protected override void OnDispose()
        {
            base.OnDispose();
            ImageSource = null;
        }
    }
}
