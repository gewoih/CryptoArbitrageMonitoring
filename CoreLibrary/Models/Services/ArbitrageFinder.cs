using CoreLibrary.Models.Enums;
using CoreLibrary.Models.Exchanges.Base;
using System.Diagnostics;

namespace CoreLibrary.Models.Services
{
	public sealed class ArbitrageFinder
	{
		private readonly List<CryptoCoin> _coins;
		private readonly List<Exchange> _exchanges;
		private readonly List<ArbitrageChain> _chains;
		private readonly int _divergencePeriod;
		private readonly decimal _amountPerTrade;

		public ArbitrageFinder(List<CryptoCoin> coins, List<Exchange> exchanges, int divergencePeriod, decimal amountPerTrade)
		{
			_coins = coins;
			_exchanges = exchanges;
			_divergencePeriod = divergencePeriod;
			_amountPerTrade = amountPerTrade;

			_chains = GetArbitrageChains(_coins, _exchanges);
		}

		public async Task<IEnumerable<ArbitrageChain>> GetUpdatedChains(decimal minimumTotalDivergence)
		{
			var tasks = _chains.Select(async chain =>
			{
				var totalDivergence = chain.GetTotalDivergence();
				var longAveragePrice = chain.FromExchangeMarketData.GetAverageMarketPriceForAmount(_amountPerTrade, TradeAction.Long);
				var shortAveragePrice = chain.ToExchangeMarketData.GetAverageMarketPriceForAmount(_amountPerTrade, TradeAction.Short);
				var fromExchangeSpread = chain.FromExchangeMarketData.Spread;
				var toExchangeSpread = chain.ToExchangeMarketData.Spread;

				if (totalDivergence >= minimumTotalDivergence &&
					longAveragePrice < shortAveragePrice &&
					(fromExchangeSpread + toExchangeSpread) < totalDivergence)
				{
					return chain;
				}
				else
				{
					return null;
				}
			});

			var chains = await Task.WhenAll(tasks);

			return chains.Where(c => c != null);
		}

		private List<ArbitrageChain> GetArbitrageChains(List<CryptoCoin> coins, List<Exchange> exchanges)
		{
			return coins
				.SelectMany(coin => GetExchangesCombinations(exchanges)
					.Where(exchangePair =>
						exchangePair.Item1.HasCoin(coin) &&
						exchangePair.Item2.MarginCoins.Contains(coin))
					.Select(exchangePair => new ArbitrageChain(coin, exchangePair.Item1, exchangePair.Item2, _divergencePeriod)))
					.ToList();
		}

		private static IEnumerable<Tuple<Exchange, Exchange>> GetExchangesCombinations(List<Exchange> exchanges)
		{
			for (int i = 0; i < exchanges.Count; i++)
			{
				for (int j = 0; j < exchanges.Count; j++)
				{
					if (j != i)
						yield return Tuple.Create(exchanges[i], exchanges[j]);
				}
			}
		}
	}
}
