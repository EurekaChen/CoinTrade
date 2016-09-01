using Eureka.CoinTrade.Base;
using Eureka.CoinTrade.MarketInfo;
using System;
using System.Windows.Input;
using Eureka.CoinTrade.Infrastructure;

namespace Eureka.CoinTrade.MarketModule.ViewModel
{
    public class ConvertCurrency : ICommand
    {
        MarketMainViewModel mainViewModel;
        public ConvertCurrency(MarketMainViewModel mainViewModel)
        {
            this.mainViewModel = mainViewModel;
        }

        #region ICommand Members
        public bool CanExecute(object parameter)
        {
            return true;
        }
        public event EventHandler CanExecuteChanged;
        public void Execute(object parameter)
        {
            decimal rate = 1;
            MarketTickers updateTickers = null;
            bool needConvert = false;
            switch (parameter.ToString())
            {
                case "BtcToCny":
                    rate = mainViewModel.BtcCny;
                    updateTickers = mainViewModel.BtcMarket;
                    needConvert = mainViewModel.IsBtcToCny;
                    break;
                case "CnyToUsd":
                    rate = 1 / mainViewModel.UsdCny;
                    updateTickers = mainViewModel.CnyMarket;
                    needConvert = mainViewModel.IsCnyToUsd;
                    break;
                case "UsdToCny":
                    rate = mainViewModel.UsdCny;
                    updateTickers = mainViewModel.UsdMarket;
                    needConvert = mainViewModel.IsUsdToCny;
                    break;
                case "LtcToCny":
                    rate = mainViewModel.LtcCny;
                    updateTickers = mainViewModel.LtcMarket;
                    needConvert = mainViewModel.IsLtcToCny;
                    break;
                case "XpmToCny":
                    rate = mainViewModel.XpmCny;
                    updateTickers = mainViewModel.XpmMarket;
                    needConvert = mainViewModel.IsXpmToCny;
                    break;
            }


            foreach (TickersByExchange tickers in updateTickers)
            {
                foreach (Ticker ticker in tickers)
                {
                    if (needConvert)
                    {
                        ticker.ConvertRate = rate;
                       // ticker.Exchange.UpdateTicker(ticker.CurrencyPair);
                        // ticker.Convert();
                        // mainViewModel.UpdateQuotaion()

                    }
                    else
                    {
                        ticker.ConvertRate = 1;
                     //   ticker.Exchange.UpdateTicker(ticker.CurrencyPair);
                        //ticker.Exchange.UpdateQuotation(ticker.CurrencyPair);
                        //  mainViewModel.UpdateQuotaion(ticker);
                    }
                }
            }
        }
        #endregion
    }
}
