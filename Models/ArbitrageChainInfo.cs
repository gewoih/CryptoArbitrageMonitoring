using CryptoArbitrageMonitoring.Models.Exchanges.Base;

namespace CryptoArbitrageMonitoring.Models
{
    public sealed class ArbitrageChainInfo
    {
        public readonly CryptoCoin Coin;
        public readonly Exchange FromExchange;
        public readonly Exchange ToExchange;

        public ArbitrageChainInfo(CryptoCoin coin, Exchange fromExchange, Exchange toExchange)
        {
            Coin = coin;
            FromExchange = fromExchange;
            ToExchange = toExchange;
        }

        public decimal FromExchangePrice => FromExchange.GetCoinPrice(Coin);
        public decimal ToExchangePrice => ToExchange.GetCoinPrice(Coin);
        public decimal Difference => Math.Round(FromExchangePrice - ToExchangePrice, 6);
        public decimal Divergence => Math.Round(FromExchangePrice / ToExchangePrice * 100 - 100, 6);
    }
}
