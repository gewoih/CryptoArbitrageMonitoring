using CoreLibrary.Extensions;
using CoreLibrary.Models;
using CoreLibrary.Models.Exchanges;
using CoreLibrary.Models.Exchanges.Base;
using CoreLibrary.Models.Trading;
using CoreLibrary.Utils;

namespace CryptoArbitrageMonitoringApp
{
	internal class Program
	{
		private static decimal exchangeCommission;
		private static decimal minimumProfit;
		private static int smaDivergencePeriod;
		private static readonly List<ArbitrageTrade> Trades = new();

		static async Task Main(string[] args)
		{
			using var httpClient = new HttpClient();
			var exchanges = new List<Exchange>
			{
				new BinanceExchange(httpClient),
				new BitfinexExchange(httpClient),
				new BitmartExchange(httpClient),
				new KucoinExchange(httpClient),
				new HuobiExchange(httpClient),
				new OkxExchange(httpClient),
				new GateioExchange(httpClient),
				new BitstampExchange(httpClient),
			};

			//Заполняем настройки из консоли
			FillUpUserParameters();

			//Запускаем и ждем 1-е обновление (чтобы все биржи были заполнены)
			StartUpdatingExchangesMarketData(exchanges);
			await WaitForAllMarketDataLoaded(exchanges);

			var coins = CoinsUtils.GetCoins();
			var arbitrageChains = GetArbitrageChains(coins, exchanges).ToList();
			await ArbitrageChainsFinder(arbitrageChains);
		}

		private static void FillUpUserParameters()
		{
			Console.Write("Exchange commission: ");
			exchangeCommission = decimal.Parse(Console.ReadLine().Replace(".", ","));

			Console.Write("Minimum profit: ");
			minimumProfit = decimal.Parse(Console.ReadLine().Replace(".", ","));

			Console.Write("SMA divergence period: ");
			smaDivergencePeriod = int.Parse(Console.ReadLine());
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
				_ = Task.Run(async () =>
				{
					while (true)
					{
						try
						{
							await exchange.UpdateCoinPrices();
						}
						catch (Exception ex)
						{
							await Task.Delay(5000);
							continue;
						}
					}
				});
			}
		}

		private static async Task ArbitrageChainsFinder(List<ArbitrageChainInfo> arbitrageChains)
		{
			Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss.fff}]: Starting arbitrage chains finder...");

			//Kucoin не может стоять на 2 месте, т.к. у них недоступна маржинальная торговля для необходимых нам монет
			arbitrageChains = arbitrageChains
				.Where(chain => chain.ToExchange.Name != "Kucoin")
				.ToList();

			while (true)
			{
				try
				{
					var topChains = arbitrageChains
						.Where(chain =>
							chain.GetCurrentDifference() != 0 &&
							chain.GetTotalDivergence() != 0 &&
							chain.GetTotalDivergence() >= exchangeCommission + minimumProfit &&
							chain.FromExchangeMarketData.GetLastTick().Ask < chain.ToExchangeMarketData.GetLastTick().Bid &&
							!Trades.Any(trade => trade.ArbitrageChain.Equals(chain) && !trade.LongTrade.IsClosed && !trade.ShortTrade.IsClosed))
						.OrderByDescending(c => c.GetTotalDivergence());

					foreach (var topChain in topChains)
					{
						Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss.fff}]: {topChain}" + Environment.NewLine);
						OpenPositionsByArbitrageChain(topChain);
					}
					
					ClosePositionsByArbitrageChains();
				}
				catch
				{
					continue;
				}
			}
		}

		private static void OpenPositionsByArbitrageChain(ArbitrageChainInfo chain)
		{
			//Есть ли незакрытая сделка с такой цепочкой?
			if (Trades.Any(trade => trade.ArbitrageChain.Equals(chain) && !trade.LongTrade.IsClosed && !trade.ShortTrade.IsClosed))
				return;

			var longTrade = new Trade();
			var shortTrade = new Trade();
			var newArbitrageTrade = new ArbitrageTrade(chain, longTrade, shortTrade);

			var longTradePrice = chain.FromExchangeMarketData.GetLastTick().Ask;
			var shortTradePrice = chain.ToExchangeMarketData.GetLastTick().Bid;

			longTrade.Open(longTradePrice, TradeType.Long);
			shortTrade.Open(shortTradePrice, TradeType.Short);

			Trades.Add(newArbitrageTrade);

			Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss.fff}]: [{newArbitrageTrade.ArbitrageChain.Coin.Name}] " +
				$"[OPEN LONG '{newArbitrageTrade.ArbitrageChain.FromExchange.Name}': {longTradePrice}$]; " +
				$"[OPEN SHORT '{newArbitrageTrade.ArbitrageChain.ToExchange.Name}': {shortTradePrice}$]" +
				$"{Environment.NewLine}");
		}

		private static void ClosePositionsByArbitrageChains()
		{
			foreach (var trade in Trades.Where(trade => !trade.LongTrade.IsClosed && !trade.ShortTrade.IsClosed))
			{
				var longTradePrice = trade.ArbitrageChain.FromExchangeMarketData.GetLastTick().Bid;
				var shortTradePrice = trade.ArbitrageChain.ToExchangeMarketData.GetLastTick().Ask;
				var estimatedProfit = trade.GetEstimatedProfit(longTradePrice, shortTradePrice);

				if (trade.ArbitrageChain.GetCurrentDivergence() < trade.ArbitrageChain.GetStandardDivergence())
                {
					trade.LongTrade.Close(longTradePrice);
					trade.ShortTrade.Close(shortTradePrice);

					SaveTradeInfo(trade);
				}
			}
		}

		private static void SaveTradeInfo(ArbitrageTrade trade)
		{
            var longTradeProfit = trade.LongTrade.Profit;
            var shortTradeProfit = trade.ShortTrade.Profit;
            var totalProfit = Math.Round(trade.ProfitPercent, 6).Normalize();
            var timeInPosition = (trade.LongTrade.TimeInTrade + trade.ShortTrade.TimeInTrade) / 2;

            Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss.fff}]: [{trade.ArbitrageChain.Coin.Name}] " +
                        $"[CLOSE LONG '{trade.ArbitrageChain.FromExchange.Name}': {trade.LongTrade.ExitPrice}$, Profit = {longTradeProfit}$]; " +
                        $"[CLOSE SHORT '{trade.ArbitrageChain.ToExchange.Name}': {trade.ShortTrade.ExitPrice}$, Profit = {shortTradeProfit}$]; " +
                        $"[Total profit = {totalProfit}%, {timeInPosition}]" +
                        $"{Environment.NewLine}");

            File.AppendAllText($"Trades [{exchangeCommission} {minimumProfit} {smaDivergencePeriod}].txt",
                $"{trade.LongTrade.EntryDateTime};" +
                $"{trade.LongTrade.ExitDateTime};" +
                $"{trade.ArbitrageChain.FromExchange.Name};" +
                $"{trade.ArbitrageChain.ToExchange.Name};" +
                $"{trade.ArbitrageChain.Coin.Name};" +
                $"{trade.LongTrade.EntryPrice};" +
                $"{trade.ShortTrade.EntryPrice};" +
                $"{trade.LongTrade.ExitPrice};" +
                $"{trade.ShortTrade.ExitPrice};" +
                $"{Environment.NewLine}");
        }

		private static IEnumerable<ArbitrageChainInfo> GetArbitrageChains(List<CryptoCoin> coins, List<Exchange> exchanges)
		{
			return coins
				.SelectMany(coin => GetExchangesCombinations(exchanges)
					.Where(exchangePair => exchangePair.Item1.HasCoin(coin) && exchangePair.Item2.HasCoin(coin))
					.Select(exchangePair => new ArbitrageChainInfo(coin, exchangePair.Item1, exchangePair.Item2, smaDivergencePeriod)));
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

