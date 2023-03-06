using CoreLibrary.Models.Enums;
using CoreLibrary.Models.MarketInfo;
using CoreLibrary.Utils;

namespace CoreLibrary.Models.Exchanges.Base
{
    public abstract class Exchange
    {
        public abstract string Name { get; }
        public abstract TickersInfo TickersInfo { get; }
        public bool IsAllMarketDataLoaded => !coinPrices.Values.Any(v => v.LastUpdate == DateTime.MinValue);
        protected bool IsCoinsWithoutMarginRemoved = false;
        protected abstract string BaseApiEndpoint { get; }
        protected readonly Dictionary<CryptoCoin, MarketData> coinPrices;
        protected readonly HttpClient httpClient;
        private bool _isMarketDataLoading = false;

        protected Exchange()
        {
            httpClient = new HttpClient();
            coinPrices = CoinsUtils.GetCoins().ToDictionary(key => key, value => new MarketData());
        }

        public void StartUpdatingMarketData()
        {
            if (!_isMarketDataLoading)
            {
                _ = Task.Run(async () =>
                {
                    while (true)
                    {
                        try
                        {
                            await UpdateCoinPrices();
                        }
                        catch (Exception ex)
                        {
                            await Task.Delay(5000);
                            continue;
                        }
                    }
                });

                _isMarketDataLoading = true;
            }
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
