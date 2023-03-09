using CoreLibrary.Models.Enums;
using CoreLibrary.Models.MarketInfo;
using CoreLibrary.Utils;

namespace CoreLibrary.Models.Exchanges.Base
{
    public abstract class Exchange
    {
        public abstract string Name { get; }
        public abstract TickersInfo TickersInfo { get; }
        public bool IsAllMarketDataLoaded => !coinPrices.Values.Any(v => v.LastTradeDateTime == DateTime.MinValue);
        protected bool IsCoinsWithoutMarginRemoved = false;
        protected readonly Dictionary<CryptoCoin, MarketData> coinPrices;
        private bool _isMarketDataLoading = false;

        protected Exchange()
        {
            coinPrices = CoinsUtils.GetCoins().ToDictionary(key => key, value => new MarketData());
        }

        public void StartUpdatingMarketData()
        {
            if (!_isMarketDataLoading)
            {
                _ = Task.Run(UpdateCoinPrices);
                _isMarketDataLoading = true;
            }
        }

        public MarketData GetCoinMarketData(CryptoCoin coin)
        {
            if (coinPrices.TryGetValue(coin, out MarketData value))
                return value;
            else
                return new();
        }

        public bool HasCoin(CryptoCoin coin)
        {
            return coinPrices.ContainsKey(coin);
        }

        public abstract Task UpdateCoinPrices();

        protected abstract Task RemoveCoinsWithoutMarginTrading();

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

        protected CryptoCoin GetCoinByTicker(string ticker)
        {
            return new(ticker.Replace($"{TickersInfo.Separator}{TickersInfo.SecondCoin.Name}", ""));
        }

        public override bool Equals(object? obj)
        {
            return obj is Exchange exchange &&
                   Name == exchange.Name;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name);
        }
    }
}
