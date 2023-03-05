using CoreLibrary.Extensions;
using CoreLibrary.Models.Exchanges.Base;
using CoreLibrary.Models.MarketInfo;

namespace CoreLibrary.Models
{
	public sealed class ArbitrageChainInfo
	{
		public readonly CryptoCoin Coin;
		public readonly Exchange FromExchange;
		public readonly Exchange ToExchange;
		public readonly int DivergencePeriod;

		public ArbitrageChainInfo(CryptoCoin coin, Exchange fromExchange, Exchange toExchange, int divergencePeriod)
		{
			Coin = coin;
			FromExchange = fromExchange;
			ToExchange = toExchange;
			DivergencePeriod = divergencePeriod;
		}

		public MarketData FromExchangeMarketData => FromExchange.GetCoinMarketData(Coin);
		public MarketData ToExchangeMarketData => ToExchange.GetCoinMarketData(Coin);

        public decimal GetTotalDivergence()
		{
			var standardDivergence = GetStandardDivergence();
			var currentDivergence = GetCurrentDivergence();

			if (standardDivergence == 0 ||
				currentDivergence == 0 ||
				currentDivergence < standardDivergence)
				return 0;

			return Math.Round(currentDivergence - standardDivergence, 6);
		}

		public decimal GetCurrentDifference()
		{
            var fromExchangeAsk = FromExchangeMarketData.GetLastTick().Ask;
            var toExchangeBid = ToExchangeMarketData.GetLastTick().Bid;

            if (fromExchangeAsk == 0 || toExchangeBid == 0)
                return 0;

			return Math.Round(fromExchangeAsk - toExchangeBid, 6);
        }

		public decimal GetStandardDivergence()
		{
            var fromExchangeSMA = FromExchangeMarketData.GetSMA(DivergencePeriod);
            var toExchangeSMA = ToExchangeMarketData.GetSMA(DivergencePeriod);

			if (fromExchangeSMA == 0 || toExchangeSMA == 0)
				return 0;

			return Math.Round(Math.Abs(fromExchangeSMA / toExchangeSMA * 100 - 100), 6);
        }

		public decimal GetCurrentDivergence()
		{
            var fromExchangeAsk = FromExchangeMarketData.GetLastTick().Ask;
            var toExchangeBid = ToExchangeMarketData.GetLastTick().Bid;

			if (fromExchangeAsk == 0 || toExchangeBid == 0)
				return 0;

            return Math.Round(Math.Abs(fromExchangeAsk / toExchangeBid * 100 - 100), 6);
        }

		public override string? ToString()
		{
			var firstExchangeLastTick = FromExchangeMarketData.GetLastTick();
			var firstExchangeBid = firstExchangeLastTick.Bid.Normalize();
			var firstExchangeAsk = firstExchangeLastTick.Ask.Normalize();
			var firstExchangeLast = firstExchangeLastTick.Last.Normalize();
			var firstExchangeSpread = firstExchangeLastTick.Spread.Normalize();

			var secondExchangeLastTick = ToExchangeMarketData.GetLastTick();
			var secondExchangeBid = secondExchangeLastTick.Bid.Normalize();
			var secondExchangeAsk = secondExchangeLastTick.Ask.Normalize();
			var secondExchangeLast = secondExchangeLastTick.Last.Normalize();
			var secondExchangeSpread = secondExchangeLastTick.Spread.Normalize();

			return $"[B:{firstExchangeBid}; A:{firstExchangeAsk}; L:{firstExchangeLast}; S:{firstExchangeSpread}%]; " +
						$"[B:{secondExchangeBid}; A:{secondExchangeAsk}; L:{secondExchangeLast}; S:{secondExchangeSpread}%]; " +
						$"DIFF:[S:{GetCurrentDifference().Normalize()}; C:{GetCurrentDifference().Normalize()}]; " +
						$"DIV:[S:{GetStandardDivergence().Normalize()}%; C:{GetCurrentDivergence().Normalize()}%; T:{GetTotalDivergence().Normalize()}%]; " +
						$"[{Coin.Name}, {FromExchange.Name}:{ToExchange.Name}]";
		}

        public override bool Equals(object? obj)
        {
            return obj is ArbitrageChainInfo info &&
                   EqualityComparer<CryptoCoin>.Default.Equals(Coin, info.Coin) &&
                   EqualityComparer<Exchange>.Default.Equals(FromExchange, info.FromExchange) &&
                   EqualityComparer<Exchange>.Default.Equals(ToExchange, info.ToExchange);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Coin, FromExchange, ToExchange);
        }
    }
}
