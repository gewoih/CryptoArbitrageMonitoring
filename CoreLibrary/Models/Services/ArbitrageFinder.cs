﻿using CoreLibrary.Models.Exchanges.Base;

namespace CoreLibrary.Models.Services
{
	public sealed class ArbitrageFinder
	{
		private readonly List<CryptoCoin> _coins;
		private readonly List<Exchange> _exchanges;
		private readonly IEnumerable<ArbitrageChain> _chains;
		private readonly int _divergencePeriod;

		public ArbitrageFinder(List<CryptoCoin> coins, List<Exchange> exchanges, int divergencePeriod)
		{
			_coins = coins;
			_exchanges = exchanges;
			_divergencePeriod = divergencePeriod;

			_chains = GetArbitrageChains(_coins, _exchanges);
		}

		public IEnumerable<ArbitrageChain> GetUpdatedChains(decimal minimumTotalDivergence)
		{
			var chains = _chains
							.Where(chain =>
								chain.GetTotalDivergence() >= minimumTotalDivergence &&
								chain.FromExchangeMarketData.GetLastTick().Ask < chain.ToExchangeMarketData.GetLastTick().Bid)
							.OrderByDescending(c => c.GetTotalDivergence());

			return chains;
		}

		private IEnumerable<ArbitrageChain> GetArbitrageChains(List<CryptoCoin> coins, List<Exchange> exchanges)
		{
			return coins
				.SelectMany(coin => GetExchangesCombinations(exchanges)
					.Where(exchangePair => 
						exchangePair.Item1.HasCoin(coin) && 
						exchangePair.Item2.HasCoin(coin) &&
                        //Kucoin не может стоять на 2 месте, т.к. у них недоступна маржинальная торговля для необходимых нам монет
                        exchangePair.Item2.Name != "Kucoin")
					.Select(exchangePair => new ArbitrageChain(coin, exchangePair.Item1, exchangePair.Item2, _divergencePeriod)));
		}

		private IEnumerable<Tuple<Exchange, Exchange>> GetExchangesCombinations(List<Exchange> exchanges)
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