using System;
using System.Reflection;
using System.Windows;
using Microsoft.Win32;

namespace Eureka.CoinTrade
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                string appName = System.IO.Path.GetFileName(Assembly.GetEntryAssembly().Location);
                string vhostName = appName.Replace("exe", "");
                vhostName = vhostName + "vshost.exe";
                const string IE_EMULATION = @"Software\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION";
                using (var fbeKey = Registry.CurrentUser.OpenSubKey(IE_EMULATION, true))
                {
                    fbeKey.SetValue(appName, 9000, RegistryValueKind.DWord);
                    fbeKey.SetValue(vhostName, 9000, RegistryValueKind.DWord);
                }
            }

            catch (Exception ex)
            {
                string message = Infrastructure.Properties.Resources.NoteChartEx + ":" + ex.Message;
                Eureka.CoinTrade.Infrastructure.EventSourceLogger.Logger.Warn(message);
                MessageBox.Show(message);
            }


            base.OnStartup(e);
            Bootstrapper bootstrapper = new Bootstrapper();
            bootstrapper.Run();
        }

    }
}
