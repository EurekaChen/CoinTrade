using System;
using System.ComponentModel.Composition;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows.Navigation;
using Eureka.CoinTrade.Infrastructure;

namespace Eureka.CoinTrade.ChartModule.View
{

    [Export]
    public partial class ChartMainView : UserControl, IView
    {
        public ChartMainView()
        {
            InitializeComponent();
            PeriodBrowser.Navigated += new NavigatedEventHandler(PeriodBrowserNavigated);
            DepthBrowser.Navigated += new NavigatedEventHandler(DepthBrowserNavigated);

            ViewModel.ChartMainViewModel.PeriodBrowser = PeriodBrowser;
            ViewModel.ChartMainViewModel.DepthBrowser = DepthBrowser;
        }


        void PeriodBrowserNavigated(object sender, NavigationEventArgs e)
        {
            SetSilent(PeriodBrowser, true); // make it silent
        }

        void DepthBrowserNavigated(object sender, NavigationEventArgs e)
        {
            SetSilent(DepthBrowser, true);
        }

        public static void SetSilent(WebBrowser browser, bool silent)
        {
            if (browser == null)
                throw new ArgumentNullException("browser");

            // get an IWebBrowser2 from the document
            IOleServiceProvider sp = browser.Document as IOleServiceProvider;
            if (sp != null)
            {
                Guid IID_IWebBrowserApp = new Guid("0002DF05-0000-0000-C000-000000000046");
                Guid IID_IWebBrowser2 = new Guid("D30C1661-CDAF-11d0-8A3E-00C04FC9E26E");

                object webBrowser;
                sp.QueryService(ref IID_IWebBrowserApp, ref IID_IWebBrowser2, out webBrowser);
                if (webBrowser != null)
                {
                    webBrowser.GetType().InvokeMember("Silent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.PutDispProperty, null, webBrowser, new object[] { silent });
                }
            }
        }


        [ComImport, Guid("6D5140C1-7436-11CE-8034-00AA006009FA"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IOleServiceProvider
        {
            [PreserveSig]
            int QueryService([In] ref Guid guidService, [In] ref Guid riid, [MarshalAs(UnmanagedType.IDispatch)] out object ppvObject);
        }

    }
}
