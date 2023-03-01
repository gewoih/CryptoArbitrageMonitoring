namespace CryptoArbitrageMonitoring.Models.Exchanges.Base
{
    public abstract class Exchange
    {
        public abstract string Name { get; }
        protected abstract string _baseApiEndpoint { get; }
        protected readonly Dictionary<CryptoCoin, decimal> CoinPrices = new();
        protected Dictionary<CryptoCoin, string> CoinsToTickers;

        public Exchange WithCoins(Dictionary<CryptoCoin, string> coinsToTickers)
        {
            CoinsToTickers = coinsToTickers;

            foreach (var coin in coinsToTickers.Keys)
            {
                CoinPrices[coin] = 0m;
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
    }
}
