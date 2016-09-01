using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Eureka.CoinTrade.Infrastructure;

namespace Eureka.CoinTrade.ConfigModule.View
{

    [Export]
    public partial class ExchangePairSelecttorView : UserControl, IView
    {
        public ExchangePairSelecttorView()
        {
            InitializeComponent();
        }
                

        /// <summary>
        /// 点击超链接到达.
        /// </summary>
        /// <param name="sender">Object the sent the event.</param>
        /// <param name="e">Navigation events arguments.</param>
        private void NavigateTo(object sender, RequestNavigateEventArgs e)
        {
            if (e.Uri != null && string.IsNullOrEmpty(e.Uri.OriginalString) == false)
            {
                string uri = e.Uri.AbsoluteUri;
                Process.Start(new ProcessStartInfo(uri));
                e.Handled = true;
            }
        }
    }
}
