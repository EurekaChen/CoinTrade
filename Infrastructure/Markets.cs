using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using Eureka.CoinTrade.ApiBase;
using Eureka.CoinTrade.Base;

namespace Eureka.CoinTrade.Infrastructure
{
    [Export]
    public class Markets
    {

        [Import]
        ApiManager apiManager;

        /// <summary>
        /// 返回某个市场的所有交易对。
        /// 当用户没有选择时返回的空列表。不是null。使用该方法时请注意为空时的处理。
        /// </summary>
        /// <param name="marketPair"></param>
        /// <returns></returns>
        public MarketTickers GetMarketTickers(MarketPair marketPair)
        {
            MarketTickers marketTickers = new MarketTickers();
            foreach (CurrencyPair pair in marketPair.QuotePairs)
            {
                TickersByExchange tickersByExchange = GetTickersByExchange(pair);
                //当货币对没有报价时！为空！根据设计，这时获取不到键值，所以就不加入到所有报价中！
                if (tickersByExchange.Count > 0)
                {
                    marketTickers.Add(tickersByExchange);
                }
            }
            return marketTickers;
        }

        ObservableCollection<MarketTickers> all;
        public ReadOnlyObservableCollection<MarketTickers> All
        {
            get
            {
                if (all == null)
                {
                    all = new ObservableCollection<MarketTickers>();

                    foreach (MarketPair marketPair in MarketPairs.All)
                    {
                        MarketTickers marketTickers = GetMarketTickers(marketPair);
                        all.Add(marketTickers);
                    }

                }
                return new ReadOnlyObservableCollection<MarketTickers>(all);
            }
        }

        public MarketTickers GetMarketTickers(string QuoteCode)
        {
            foreach (var item in All)
            {
                if (item.Quote.Code == QuoteCode)
                {
                    return item;
                }
            }
            return null;
        }

        public void UpdateAll()
        {
            foreach (var marketTickers in All)
            {
                UpdateMarketTickers(marketTickers);
            }
        }


        public void UpdateMarketTickers(MarketTickers tickers)
        {
            foreach (TickersByExchange exchangeTickers in tickers)
            {
                UpdateExchangeTickers(exchangeTickers);
            }
        }

        //说明:为什么没有把这些更新放到相应的类里是因为apiManager只出现在Markets这个类中！
        private void UpdateExchangeTickers(TickersByExchange exchangeTickers)
        {
            foreach (var ticker in exchangeTickers)
            {
                ITickerApi tickerUpdater = apiManager.GetApi<ITickerApi>(ticker.Exchange.AbbrName);
                var refTicker = ticker;
                tickerUpdater.UpdateTicker(ref refTicker);
            }
        }

        private TickersByExchange GetTickersByExchange(CurrencyPair pair)
        {
            TickersByExchange tickers = new TickersByExchange();
            foreach (ExchangeApi tradeApi in apiManager.AllExchangeApi)
            {
                if (tradeApi.SelectedPairs.Contains(pair))
                {
                    var ticker = tradeApi.SelectedTickers[pair.ToString()];
                    tickers.Add(ticker);
                }
            }
            return tickers;
        }

    }
}
