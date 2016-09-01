using System.ComponentModel.Composition;
using System.Windows.Controls;
using Eureka.CoinTrade.Infrastructure;

namespace Eureka.CoinTrade.AlertModule.View
{
    [Export]
    public partial class MultipleView : UserControl, IView
    {
        public MultipleView()
        {
            InitializeComponent();
        }

        //private void ChangeText(object sender, RoutedEventArgs e)
        //{
        //    ToggleButton button = sender as ToggleButton;
        //    if (button.IsChecked ?? false)
        //    {
        //        button.Content = "停止提醒";
        //    }
        //    else
        //    {
        //        button.Content = "开启提醒";
        //    }
        //}
    }
}
