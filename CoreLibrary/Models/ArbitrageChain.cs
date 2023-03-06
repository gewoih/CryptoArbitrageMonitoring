using CoreLibrary.Extensions;
using CoreLibrary.Models.Exchanges.Base;
using CoreLibrary.Models.MarketInfo;

namespace CoreLibrary.Models
{
	public sealed class ArbitrageChain
	{
		public readonly CryptoCoin Coin;
		public readonly Exchange FromExchange;
		public readonly Exchange ToExchange;
		private readonly int _divergencePeriod;

		public ArbitrageChain(CryptoCoin coin, Exchange fromExchange, Exchange toExchange, int divergencePeriod)
		{
			Coin = coin;
			FromExchange = fromExchange;
			ToExchange = toExchange;
			_divergencePeriod = divergencePeriod;
		}

		public MarketData FromExchangeMarketData => FromExchange.GetCoinMarketData(Coin);
		public MarketData ToExchangeMarketData => ToExchange.GetCoinMarketData(Coin);

		public decimal GetStandardDivergence()
		{
            var fromExchangeSMA = FromExchangeMarketData.GetSMA(_divergencePeriod);
            var toExchangeSMA = ToExchangeMarketData.GetSMA(_divergencePeriod);

			if (fromExchangeSMA == 0 || toExchangeSMA == 0)
				return 0;

			return Math.Abs(fromExchangeSMA / toExchangeSMA * 100 - 100);
        }

		public decimal GetCurrentDivergence()
		{
            var fromExchangeAsk = FromExchangeMarketData.GetLastTick().Ask;
            var toExchangeBid = ToExchangeMarketData.GetLastTick().Bid;

			if (fromExchangeAsk == 0 || toExchangeBid == 0)
				return 0;

            return Math.Abs(fromExchangeAsk / toExchangeBid * 100 - 100);
        }
        
		public decimal GetTotalDivergence()
		{
			var standardDivergence = GetStandardDivergence();
			var currentDivergence = GetCurrentDivergence();

			if (standardDivergence == 0 || currentDivergence == 0)
				return 0;

			return currentDivergence - standardDivergence;
		}

		public decimal GetCurrentDifference()
		{
            var fromExchangeAsk = FromExchangeMarketData.GetLastTick().Ask;
            var toExchangeBid = ToExchangeMarketData.GetLastTick().Bid;

            if (fromExchangeAsk == 0 || toExchangeBid == 0)
                return 0;

			return fromExchangeAsk - toExchangeBid;
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

			var currentDifference = GetCurrentDifference().Normalize();
			var standardDivergence = Math.Round(GetStandardDivergence().Normalize(), 2);
			var currentDivergence = Math.Round(GetCurrentDivergence().Normalize(), 2);
			var totalDivergence = Math.Round(GetTotalDivergence().Normalize(), 2);

            return $"[B:{firstExchangeBid}; A:{firstExchangeAsk}; L:{firstExchangeLast}; S:{firstExchangeSpread}%]; " +
						$"[B:{secondExchangeBid}; A:{secondExchangeAsk}; L:{secondExchangeLast}; S:{secondExchangeSpread}%]; " +
						$"[DIFF: {currentDifference}$; " +
						$"S:{standardDivergence}%; " +
						$"C:{currentDivergence}%; " +
						$"T:{totalDivergence}%];";
		}

		public override bool Equals(object? obj)
        {
            return obj is ArbitrageChain info &&
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
