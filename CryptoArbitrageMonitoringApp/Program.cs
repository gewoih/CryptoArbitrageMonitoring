using CoreLibrary.Models.Exchanges;
using CoreLibrary.Models.Exchanges.Base;
using CoreLibrary.Models.Services;
using CoreLibrary.Models.Trading;
using CoreLibrary.Utils;

namespace CryptoArbitrageMonitoringApp
{
	internal class Program
	{
		private static decimal _minimumTotalDivergence;
		private static int _divergencePeriod;
		private static int _minimumSecondsInTrade;
		private static ArbitrageFinder _arbitrageFinder;
		private static ArbitrageTradesManager _tradesManager;

		static async Task Main(string[] args)
		{
			var exchanges = new List<Exchange>
			{
				new BinanceExchange(),
				//new BitfinexExchange(),
				new BitmartExchange(),
				new KucoinExchange(),
				new HuobiExchange(),
				new OkxExchange(),
				new GateioExchange(),
				new BitstampExchange(),
			};

			FillUpConsoleParameters();

			StartUpdatingExchangesMarketData(exchanges);
			await WaitForAllMarketDataLoaded(exchanges);

			var coins = CoinsUtils.GetCoins();
			_arbitrageFinder = new ArbitrageFinder(coins, exchanges, _divergencePeriod);
			_tradesManager = new ArbitrageTradesManager(_minimumSecondsInTrade);
			_tradesManager.OnTradeOpened += ArbitrageTradesManager_OnTradeOpened;
			_tradesManager.OnTradeClosed += ArbitrageTradesManager_OnTradeClosed;
			
			await StartFindingArbitrageChains();
		}

		private static void FillUpConsoleParameters()
		{
			Console.Write("Minimum total divergence: ");
			_minimumTotalDivergence = decimal.Parse(Console.ReadLine().Replace(".", ","));

			Console.Write("SMA divergence period: ");
			_divergencePeriod = int.Parse(Console.ReadLine());

			Console.Write("Minimum seconds in trade: ");
			_minimumSecondsInTrade = int.Parse(Console.ReadLine());
		}

		private static async Task WaitForAllMarketDataLoaded(List<Exchange> exchanges)
		{
			while (true)
			{
				if (exchanges.Any(exchange => !exchange.IsAllMarketDataLoaded))
				{
					Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss.fff}]: Waiting for market data loading...");
					await Task.Delay(1000);
					continue;
				}

				Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss.fff}]: Market data loaded successfully!");
				return;
			}
		}

		private static void StartUpdatingExchangesMarketData(List<Exchange> exchanges)
		{
			Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss.fff}]: Starting exchanges market data updating...");
			foreach (var exchange in exchanges)
			{
				exchange.StartUpdatingMarketData();
			}
		}

		private static async Task StartFindingArbitrageChains()
		{
			Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss.fff}]: Starting arbitrage chains finder...");
			
			while (true)
			{
				try
				{
					var topChains = _arbitrageFinder.GetUpdatedChains(_minimumTotalDivergence);

					foreach (var topChain in topChains)
					{
						var newTrade = _tradesManager.TryOpenPositionByArbitrageChain(topChain);
					}
				}
				catch
				{
					continue;
				}
			}
		}

		private static void ArbitrageTradesManager_OnTradeOpened(ArbitrageTrade trade)
		{
            Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss.fff}]: OPENED TRADE {trade} {Environment.NewLine}");
        }

		private static void ArbitrageTradesManager_OnTradeClosed(ArbitrageTrade trade)
		{
			Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss.fff}]: CLOSED TRADE {trade} {Environment.NewLine}");

            File.AppendAllText($"Trades [{_minimumTotalDivergence} {_divergencePeriod}].txt",
                $"{trade.LongTrade.EntryDateTime};" +
                $"{trade.LongTrade.ExitDateTime};" +
				$"{trade.TimeInTrade.Seconds};" +
                $"{trade.ArbitrageChain.FromExchange.Name};" +
                $"{trade.ArbitrageChain.ToExchange.Name};" +
                $"{trade.ArbitrageChain.Coin.Name};" +
                $"{trade.LongTrade.EntryPrice};" +
                $"{trade.ShortTrade.EntryPrice};" +
                $"{trade.LongTrade.ExitPrice};" +
                $"{trade.ShortTrade.ExitPrice};" +
                $"{Environment.NewLine}");
        }
	}
}

