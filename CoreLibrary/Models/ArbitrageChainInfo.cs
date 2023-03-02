using CoreLibrary.Extensions;
using CoreLibrary.Models.Exchanges.Base;

namespace CoreLibrary.Models
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

        public override string? ToString()
        {
            var firstExchangeBid = FromExchangeMarketData.Bid.Normalize();
            var firstExchangeAsk = FromExchangeMarketData.Ask.Normalize();
            var firstExchangeLast = FromExchangeMarketData.Last.Normalize();
            var firstExchangeSpread = FromExchangeMarketData.Spread.Normalize();

            var secondExchangeBid = ToExchangeMarketData.Bid.Normalize();
            var secondExchangeAsk = ToExchangeMarketData.Ask.Normalize();
            var secondExchangeLast = ToExchangeMarketData.Last.Normalize();
            var secondExchangeSpread = ToExchangeMarketData.Spread.Normalize();

            return $"(B:{firstExchangeBid}; A:{firstExchangeAsk}; L:{firstExchangeLast}; S:{firstExchangeSpread}%); " +
                        $"(B:{secondExchangeBid}; A:{secondExchangeAsk}; L:{secondExchangeLast}; S:{secondExchangeSpread}%); " +
                        $"{Difference.Normalize()}; " +
                        $"{Divergence.Normalize()}%; " +
                        $"{Coin.Name}" +
                        $"({FromExchange.Name}-{ToExchange.Name})";
        }
    }
}
