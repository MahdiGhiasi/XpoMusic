using ModernSpotifyUWP.Classes;
using ModernSpotifyUWP.Classes.Model;
using ModernSpotifyUWP.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace ModernSpotifyUWP.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        public SettingsViewModel()
        {
            Themes = ThemeHelper.GetThemes();
            Languages = LanguageHelper.GetLanguages();
        }

        public List<Language> Languages { get; set; }

        private Language selectedLanguage = LocalConfiguration.Language;
        public Language SelectedLanguage
        {
            get
            {
                return selectedLanguage;
            }
            set
            {
                if (selectedLanguage == value)
                    return;

                selectedLanguage = value;
                FirePropertyChangedEvent(nameof(SelectedLanguage));
                LocalConfiguration.Language = value;
                LanguageRestartNeededNoticeVisibility = Visibility.Visible;
                AnalyticsHelper.Log("settingChange", "language", value.ToString());
            }
        }

        private Visibility languageRestartNeededNoticeVisibility = Visibility.Collapsed;
        public Visibility LanguageRestartNeededNoticeVisibility
        {
            get
            {
                return languageRestartNeededNoticeVisibility;
            }
            set
            {
                languageRestartNeededNoticeVisibility = value;
                FirePropertyChangedEvent(nameof(LanguageRestartNeededNoticeVisibility));
            }
        }

        public List<Theme> Themes { get; set; }

        private Theme selectedTheme = LocalConfiguration.Theme;
        public Theme SelectedTheme
        {
            get
            {
                return selectedTheme;
            }
            set
            {
                if (selectedTheme == value)
                    return;

                selectedTheme = value;
                FirePropertyChangedEvent(nameof(SelectedTheme));
                LocalConfiguration.Theme = value;
                ThemeRestartNeededNoticeVisibility = Visibility.Visible;
                AnalyticsHelper.Log("settingChange", "theme", value.ToString());
            }
        }

        private Visibility themeRestartNeededNoticeVisibility = Visibility.Collapsed;
        public Visibility ThemeRestartNeededNoticeVisibility
        {
            get
            {
                return themeRestartNeededNoticeVisibility;
            }
            set
            {
                themeRestartNeededNoticeVisibility = value;
                FirePropertyChangedEvent(nameof(ThemeRestartNeededNoticeVisibility));
            }
        }

        public bool OpenInMiniViewByCortana
        {
            get
            {
                return LocalConfiguration.OpenInMiniViewByCortana;
            }
            set
            {
                LocalConfiguration.OpenInMiniViewByCortana = value;
                FirePropertyChangedEvent(nameof(OpenInMiniViewByCortana));
                AnalyticsHelper.Log("settingChange", "openInMiniViewByCortana", value.ToString());
            }
        }

    }
}
