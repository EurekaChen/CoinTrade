using System.ComponentModel.Composition;
using System.Windows.Controls;
using Eureka.CoinTrade.Infrastructure;

namespace Eureka.CoinTrade.StatusModule.View
{   
    [Export]
    public partial class StatusMainView : UserControl,IView
    {
        public StatusMainView()
        {
            InitializeComponent();
        }
    }
}
