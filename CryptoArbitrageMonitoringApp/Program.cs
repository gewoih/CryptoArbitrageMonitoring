using CoreLibrary.Models;
using CoreLibrary.Models.Exchanges;
using CoreLibrary.Models.Exchanges.Base;
using CoreLibrary.Models.Services;
using CoreLibrary.Models.Trading;
using CoreLibrary.Utils;
using System.Drawing;

namespace CryptoArbitrageMonitoringApp
{
	internal class Program
	{
		static async Task Main(string[] args)
		{
            var exchanges = new List<Exchange>
            {
                new BinanceExchange(),
				new BitfinexExchange(),
				new BitmartExchange(),
				new KucoinExchange(),
				new HuobiExchange(),
				new OkxExchange(),
				new GateioExchange(),
				new BitstampExchange(),
			};
            var coins = CoinsUtils.GetCoins();

            StartUpdatingExchangesMarketData(exchanges);
			await WaitForAllMarketDataLoaded(exchanges);
			
			CreateStrategies(coins, exchanges);

			Console.ReadLine();
		}

		private static void CreateStrategies(List<CryptoCoin> coins, List<Exchange> exchanges)
		{
            Console.Write("Number of strategies: ");
			var strategiesNumber = int.Parse(Console.ReadLine());

			for (int i = 0; i < strategiesNumber; i++)
			{
                Console.Write($"[Strategy {i+1}] Minimum total divergence: ");
                var minimumTotalDivergence = decimal.Parse(Console.ReadLine().Replace(".", ","));

                Console.Write($"[Strategy {i+1}] SMA divergence period: ");
                var divergencePeriod = int.Parse(Console.ReadLine());

                Console.Write($"[Strategy {i+1}] Minimum seconds in trade: ");
                var minimumSecondsInTrade = int.Parse(Console.ReadLine());

                var arbitrageStrategy = new ArbitrageStrategy(coins, exchanges, minimumTotalDivergence, divergencePeriod, minimumSecondsInTrade);

				arbitrageStrategy.Start();
            }
        }

		private static async Task WaitForAllMarketDataLoaded(List<Exchange> exchanges)
		{
			while (true)
			{
				var notReadyExchange = exchanges.FirstOrDefault(e => !e.IsAllMarketDataLoaded);

				if (notReadyExchange != null)
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
	}
}

