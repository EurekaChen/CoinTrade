using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Eureka.CoinTrade.Infrastructure;
using Eureka.Localization;
using MahApps.Metro;
using MahApps.Metro.Controls;

namespace Eureka.CoinTrade
{
    [Export]
    public partial class Shell : MetroWindow
    {
        private Option option;
        public Shell()
        {
            InitializeComponent();
            option = Setting.Singleton.Option;
            if (!string.IsNullOrEmpty(option.LanguageCode))
            {
                CultureManager.UICulture = new CultureInfo(option.LanguageCode);
            }
            ChangeAccent();
        }

        private void ChangeAccent()
        {
            ChangeAccent(option.Accent, option.Theme);
        }

        private void ChangeAccent(string accentName, string themeName)
        {
            Theme theme = Theme.Light;
            if (themeName == "Dark") theme = Theme.Dark;
            Accent accent = ThemeManager.DefaultAccents.First(x => x.Name == accentName);
            ThemeManager.ChangeTheme(Application.Current, accent, theme);
        }

        private void ChangeTheme(object sender, RoutedEventArgs e)
        {
            var headerName = (string)((MenuItem)sender).Name;
            option.Theme = headerName;

            ChangeAccent();
            option.Save();
        }


        private void ChangeAccent(object sender, RoutedEventArgs e)
        {
            var headerName = (string)((MenuItem)sender).Name;
            option.Accent = headerName;
            ChangeAccent();
            option.Save();
        }


        private void OpenAboutWindow(object sender, RoutedEventArgs e)
        {
            AboutWindow aboutWindow = new AboutWindow(this);
            aboutWindow.ShowDialog();

        }

        private void Close(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ChangeToEnglish(object sender, RoutedEventArgs e)
        {
            CultureManager.UICulture = new CultureInfo("en");
            option.LanguageCode = "en";
            option.Save();
        }

        private void ChangeToSimChinese(object sender, RoutedEventArgs e)
        {
            CultureManager.UICulture = new CultureInfo("zh-Hans");
            option.LanguageCode = "zh-Hans";
            option.Save();

        }

        private void ChangeToTraChinese(object sender, RoutedEventArgs e)
        {
            CultureManager.UICulture = new CultureInfo("zh-Hant");
            option.LanguageCode = "zh-Hant";
            option.Save();
        }

        private void CloseCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }

        private void CloseCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
    }
}
