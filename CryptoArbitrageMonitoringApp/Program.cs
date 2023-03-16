using CoreLibrary.Models;
using CoreLibrary.Models.Exchanges;
using CoreLibrary.Models.Exchanges.Base;
using CoreLibrary.Models.Reporters;
using CoreLibrary.Models.Services;
using CoreLibrary.Utils;

namespace CryptoArbitrageMonitoringApp
{
	internal class Program
	{
		static async Task Main(string[] args)
		{
			var exchanges = new List<Exchange>
			{
				new BitfinexExchange(),
				new BinanceExchange(),
				new KucoinExchange(),
				new KrakenExchange(),
				new HuobiExchange(),
				new BybitExchange(),
				new BittrexExchange()
			};

			var coins = CoinsUtils.GetCoins();

			var tradeReporter = new DiscordTradeReporter(1085501862939205732);
			await tradeReporter.InitializeAsync();
			await Task.Delay(5000);

			StartUpdatingExchangesMarketData(exchanges);
			await CreateStrategies(coins, exchanges, tradeReporter);

			await Task.Delay(-1);
		}

		private static async Task CreateStrategies(List<CryptoCoin> coins, List<Exchange> exchanges, DiscordTradeReporter tradeReporter)
		{
			Console.Write("Number of strategies: ");
			var strategiesNumber = int.Parse(Console.ReadLine());

			for (int i = 0; i < strategiesNumber; i++)
			{
				Console.Write($"[Strategy {i + 1}] Minimum total divergence (%): ");
				var minimumTotalDivergence = decimal.Parse(Console.ReadLine());

				Console.Write($"[Strategy {i + 1}] SMA divergence period: ");
				var divergencePeriod = int.Parse(Console.ReadLine());

				Console.Write($"[Strategy {i + 1}] Minimum seconds in trade: ");
				var minimumSecondsInTrade = int.Parse(Console.ReadLine());

				Console.Write($"[Strategy {i + 1}] Minimum seconds of chain holding: ");
				var minimumSecondsOfChainHolding = int.Parse(Console.ReadLine());

				Console.Write($"[Strategy {i + 1}] Take-profit (%): ");
				var takeProfit = decimal.Parse(Console.ReadLine());

				Console.Write($"[Strategy {i + 1}] Stop-loss (%): ");
				var stopLoss = decimal.Parse(Console.ReadLine());

				Console.Write($"[Strategy {i + 1}] Amount per trade ($): ");
				var amountPerTrade = decimal.Parse(Console.ReadLine());

				var arbitrageStrategy = new ArbitrageStrategy(tradeReporter, coins, exchanges, minimumTotalDivergence,
					divergencePeriod, minimumSecondsInTrade, takeProfit, stopLoss, amountPerTrade, minimumSecondsOfChainHolding);

				await arbitrageStrategy.StartAsync();
			}
		}

		private static void StartUpdatingExchangesMarketData(List<Exchange> exchanges)
		{
			Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss.fff}]: Starting exchanges market data updating...");
			foreach (var exchange in exchanges)
			{
				var marketDataUpdatingThread = new Thread(async () =>
				{
					try
					{
						await exchange.StartUpdatingMarketData();
					}
					catch
					{
						Console.WriteLine($"ERROR WITH '{exchange.Name}' MARKET DATA!");
					}
				});

				marketDataUpdatingThread.Name = $"{exchange.Name} updater";
				marketDataUpdatingThread.Start();
			}
		}
	}
}
