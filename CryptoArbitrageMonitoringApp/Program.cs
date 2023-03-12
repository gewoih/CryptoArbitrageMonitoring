using CoreLibrary.Models;
using CoreLibrary.Models.Exchanges;
using CoreLibrary.Models.Exchanges.Base;
using CoreLibrary.Models.Services;
using CoreLibrary.Utils;

namespace CryptoArbitrageMonitoringApp
{
	internal class Program
	{
		static void Main(string[] args)
		{
			var exchanges = new List<Exchange>
			{
				new BitfinexExchange(),
				new BinanceExchange(),
				new KucoinExchange(),
				new KrakenExchange(),
				new HuobiExchange(),
				new BybitExchange(),
				new OkxExchange(),
				new GateioExchange(),
			};

			var coins = CoinsUtils.GetCoins();

			StartUpdatingExchangesMarketData(exchanges);
			CreateStrategies(coins, exchanges);

			Console.ReadLine();
		}

		private static void CreateStrategies(List<CryptoCoin> coins, List<Exchange> exchanges)
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

				var arbitrageStrategy = new ArbitrageStrategy(coins, exchanges, minimumTotalDivergence, 
					divergencePeriod, minimumSecondsInTrade, takeProfit, stopLoss, amountPerTrade, minimumSecondsOfChainHolding);

				arbitrageStrategy.Start();
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
	}
}

