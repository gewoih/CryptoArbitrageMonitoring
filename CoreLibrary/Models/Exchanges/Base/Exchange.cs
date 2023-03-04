using CoreLibrary.Models.Enums;
using CoreLibrary.Models.MarketInfo;
using CoreLibrary.Utils;

namespace CoreLibrary.Models.Exchanges.Base
{
    public abstract class Exchange
    {
        public abstract string Name { get; }
        public abstract ExchangeTickersInfo TickersInfo { get; }
        public bool IsAllMarketDataLoaded => !coinPrices.Values.Any(v => v.LastUpdate == DateTime.MinValue);
        protected abstract string BaseApiEndpoint { get; }
        protected readonly Dictionary<CryptoCoin, MarketData> coinPrices;
        protected readonly HttpClient httpClient;

        protected Exchange(HttpClient httpClient)
        {
            this.httpClient = httpClient;
            coinPrices = CoinsUtils.GetCoins().ToDictionary(key => key, value => new MarketData());
        }

        public MarketData GetCoinMarketData(CryptoCoin coin)
        {
            return coinPrices[coin];
        }

        public bool HasCoin(CryptoCoin coin)
        {
            return coinPrices.ContainsKey(coin);
        }

        public abstract Task UpdateCoinPrices();

        protected string GetTickerByCoin(CryptoCoin coin)
        {
            var ticker = coin.Name + TickersInfo.Separator + TickersInfo.SecondCoin.Name;

            if (TickersInfo.CaseType == CaseType.Uppercase)
                ticker = ticker.ToUpper();
            else if (TickersInfo.CaseType == CaseType.Lowercase)
                ticker = ticker.ToLower();

            ticker = TickersInfo.Prefix + ticker;

            return ticker;
        }

        public override bool Equals(object? obj)
        {
            return obj is Exchange exchange &&
                   Name == exchange.Name;
        }
    }
}
