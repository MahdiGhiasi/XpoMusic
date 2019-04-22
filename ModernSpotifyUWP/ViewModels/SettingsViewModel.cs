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
            Languages = LanguageHelper.GetLanguages();
            SelectedLanguage = LocalConfiguration.Language;
        }

        public List<Language> Languages { get; set; }

        private Language selectedLanguage;
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
    }
}
