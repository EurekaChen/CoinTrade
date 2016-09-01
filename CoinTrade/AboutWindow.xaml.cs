using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;
using MahApps.Metro.Controls;

namespace Eureka.CoinTrade
{
    public partial class AboutWindow : MetroWindow
    {
        protected AboutWindow()
        {
            InitializeComponent();
            this.DataContext = new AboutViewModel();
        }

        /// <summary>
        /// 构造函数，指定“关于”窗口的父窗口.
        /// </summary>       
        public AboutWindow(Window parent)
            : this()
        {
            this.Owner = parent;
        }

        /// <summary>
        /// 导航到关于窗口串的超链接.
        /// </summary>     
        private void hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
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
