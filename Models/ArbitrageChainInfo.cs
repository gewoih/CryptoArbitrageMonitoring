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

        public MarketData FromExchangeMarketData => FromExchange.GetCoinMarketData(Coin);
        public MarketData ToExchangeMarketData => ToExchange.GetCoinMarketData(Coin);
        public decimal Difference => Math.Round(FromExchangeMarketData.Last - ToExchangeMarketData.Last, 6);
        public decimal Divergence => FromExchangeMarketData.Last != 0 && ToExchangeMarketData.Last != 0 ? Math.Round(FromExchangeMarketData.Last / ToExchangeMarketData.Last * 100 - 100, 6) : 0;
    }
}
