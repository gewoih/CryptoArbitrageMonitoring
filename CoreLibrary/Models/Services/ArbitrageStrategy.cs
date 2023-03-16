using CoreLibrary.Models.Exchanges.Base;
using CoreLibrary.Models.Reporters;
using CoreLibrary.Models.Trading;
using System.Collections.Concurrent;

namespace CoreLibrary.Models.Services
{
	public sealed class ArbitrageStrategy
	{
		public readonly decimal MinimumTotalDivergence;
		public readonly int DivergencePeriod;
		public readonly int MinimumSecondsInTrade;
		public readonly decimal TakeProfit;
		public readonly decimal StopLoss;
		public readonly int MinimumSecondsOfChainHolding;
		private readonly ArbitrageFinder _arbitrageFinder;
		private readonly ArbitrageTradesManager _tradesManager;
		private readonly ConcurrentDictionary<ArbitrageChain, DateTime> _arbitrageChainsDiscoveryTimes;
		private readonly DiscordTradeReporter _tradeReporter;

		public ArbitrageStrategy(DiscordTradeReporter tradeReporter, List<CryptoCoin> coins, List<Exchange> exchanges, decimal minimumTotalDivergence, int divergencePeriod,
			int minimumSecondsInTrade, decimal takeProfit, decimal stopLoss, decimal amountPerTrade, int minimumSecondsOfChainHolding)
		{
			MinimumTotalDivergence = minimumTotalDivergence;
			DivergencePeriod = divergencePeriod;
			MinimumSecondsInTrade = minimumSecondsInTrade;
			TakeProfit = takeProfit;
			StopLoss = stopLoss;
			MinimumSecondsOfChainHolding = minimumSecondsOfChainHolding;
			_tradeReporter = tradeReporter;
			_arbitrageChainsDiscoveryTimes = new();

			_arbitrageFinder = new ArbitrageFinder(coins, exchanges, divergencePeriod, amountPerTrade);
			_tradesManager = new ArbitrageTradesManager(minimumSecondsInTrade, takeProfit, amountPerTrade, stopLoss);

			_tradesManager.OnTradeOpened += ArbitrageTradesManager_OnTradeOpened;
			_tradesManager.OnTradeClosed += ArbitrageTradesManager_OnTradeClosed;
		}

		public async Task StartAsync()
		{
			Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss.fff}]: Starting arbitrage chains finder...");

			await _tradeReporter.StartReporting();

			var arbitrageChainsFinderThread = new Thread(async () =>
			{
				while (true)
				{
					var bestChain = await _arbitrageFinder.GetBestChain(MinimumTotalDivergence,
						_arbitrageChainsDiscoveryTimes.Select(c => c.Key)
							.Concat(_tradesManager.GetOpenTradesArbitrageChains()));

					if (bestChain is not null)
						await NotifyIfNewChainDiscovered(bestChain);
				}
			});

			var arbitrageTradesHandler = new Thread(() =>
			{
				while (true)
				{
					TryOpenTrades();
					Thread.Sleep(1);
				}
			});

			arbitrageChainsFinderThread.Start();
			arbitrageTradesHandler.Start();
		}

		private void TryOpenTrades()
		{
			foreach (var chain in _arbitrageChainsDiscoveryTimes)
			{
				if (DateTime.Now - chain.Value < TimeSpan.FromSeconds(MinimumSecondsOfChainHolding))
					continue;

				Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss.fff}]: CHAIN STAYED MORE THAN " +
					$"{MinimumSecondsOfChainHolding} SECONDS. TRYING OPEN THE TRADE.");

				if (_arbitrageFinder.IsChainRelevant(chain.Key, MinimumTotalDivergence))
					_tradesManager.TryOpenTradeByArbitrageChain(chain.Key);
				
				_ = _tradeReporter.RemoveSignal(chain.Key);
				_arbitrageChainsDiscoveryTimes.TryRemove(chain.Key, out _);
			}
		}

		private async Task<bool> NotifyIfNewChainDiscovered(ArbitrageChain chain)
		{
			if (!_arbitrageChainsDiscoveryTimes.ContainsKey(chain) && !_tradesManager.IsAnyOpenedTradeForChain(chain))
			{
				_arbitrageChainsDiscoveryTimes[chain] = DateTime.Now;
				
				Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss.fff}]: NEW CHAIN WAS DISCOVERED: {chain} {Environment.NewLine}");
				_ = _tradeReporter.SendSignalInfo(chain);
				
				return true;
			}
			return false;
		}

		private async Task ArbitrageTradesManager_OnTradeOpened(ArbitrageTrade trade)
		{
			Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss.fff}]: OPENED TRADE {trade} {Environment.NewLine}");

			_ = _tradeReporter.SendOpenedTradeInfo(trade);
		}

		private async Task ArbitrageTradesManager_OnTradeClosed(ArbitrageTrade trade)
		{
			_ = _tradeReporter.SendClosedTradeInfo(trade);

			var arbitrageTradeProfitWithComission = (trade.LongTrade.Profit + trade.ShortTrade.Profit - trade.Comission) /
													(trade.LongTrade.EntryPrice + trade.ShortTrade.EntryPrice)
													* 100;

			Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss.fff}]: CLOSED TRADE {trade} {Environment.NewLine}");
			
			File.AppendAllText($"Trades [" +
					$"{MinimumTotalDivergence.ToString().Replace(".", ",")} " +
					$"{DivergencePeriod} " +
					$"{MinimumSecondsInTrade} " +
					$"{MinimumSecondsOfChainHolding} " +
					$"{TakeProfit.ToString().Replace(".", ",")} " +
					$"{StopLoss.ToString().Replace(".", ",")}].txt",
				$"{trade.LongTrade.EntryDateTime};" +
				$"{trade.LongTrade.ExitDateTime};" +
				$"{(int)trade.TimeInTrade.TotalSeconds};" +
				$"{trade.ArbitrageChain.FromExchange.Name};" +
				$"{trade.ArbitrageChain.ToExchange.Name};" +
				$"{trade.ArbitrageChain.Coin.Name};" +
				$"{trade.LongTrade.EntryPrice};" +
				$"{trade.LongTrade.Amount};" +
				$"{trade.ShortTrade.EntryPrice};" +
				$"{trade.ShortTrade.Amount};" +
				$"{trade.LongTrade.ExitPrice};" +
				$"{trade.ShortTrade.ExitPrice};" +
				$"{trade.LongTrade.Profit};" +
				$"{trade.ShortTrade.Profit};" +
				$"{trade.LongTrade.Profit + trade.ShortTrade.Profit};" +
				$"{trade.Comission};" +
				$"{arbitrageTradeProfitWithComission}" +
				$"{Environment.NewLine}");
		}
	}
}
