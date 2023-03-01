using CryptoArbitrageMonitoring.Models.Enums;

namespace CryptoArbitrageMonitoring.Models.Exchanges.Base
{
    public abstract class Exchange
    {
        public abstract string Name { get; }
        public ExchangeTickersInfo TickersInfo { get; set; }
        protected abstract string _baseApiEndpoint { get; }
        protected readonly Dictionary<CryptoCoin, decimal> CoinPrices;

        public Exchange(List<CryptoCoin> coins, ExchangeTickersInfo tickersInfo)
        {
            TickersInfo = tickersInfo;
            CoinPrices = coins.ToDictionary(key => key, value => 0m);
        }

        public Exchange RemoveCoins(List<CryptoCoin> coins)
        {
            foreach (var coin in coins)
            {
                CoinPrices.Remove(coin);
            }

            return this;
        }

        public decimal GetCoinPrice(CryptoCoin coin)
        {
            if (CoinPrices.TryGetValue(coin, out decimal price)) 
                return price;

            return decimal.Zero;
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

            return ticker;
        }
    }
}
