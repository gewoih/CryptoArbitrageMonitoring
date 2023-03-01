using CryptoArbitrageMonitoring.Models.Enums;
using System.Net.NetworkInformation;

namespace CryptoArbitrageMonitoring.Models.Exchanges.Base
{
    public abstract class Exchange
    {
        public abstract string Name { get; }
        public ExchangeTickersInfo TickersInfo { get; set; }
        protected abstract string _baseApiEndpoint { get; }
        protected readonly Dictionary<CryptoCoin, MarketData> CoinPrices;

        public Exchange(List<CryptoCoin> coins, ExchangeTickersInfo tickersInfo)
        {
            TickersInfo = tickersInfo;
            CoinPrices = coins.ToDictionary(key => key, value => new MarketData());
        }

        public Exchange RemoveCoins(List<CryptoCoin> coins)
        {
            foreach (var coin in coins)
            {
                CoinPrices.Remove(coin);
            }

            return this;
        }

        public MarketData GetCoinMarketData(CryptoCoin coin)
        {
            if (CoinPrices.TryGetValue(coin, out MarketData marketData)) 
                return marketData;

            return new MarketData();
        }

        public bool HasCoin(CryptoCoin coin)
        {
            return CoinPrices.ContainsKey(coin);
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
    }
}
