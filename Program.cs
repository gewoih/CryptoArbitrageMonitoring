using CryptoArbitrageMonitoring.Models;
using CryptoArbitrageMonitoring.Models.Enums;
using CryptoArbitrageMonitoring.Models.Exchanges;
using CryptoArbitrageMonitoring.Models.Exchanges.Base;
using System.Linq;
using System.Net.Http.Headers;

namespace BinanceMonitoring
{
    internal class Program
	{
        private static CryptoCoin usdtCoin = new("USDT");
        private static CryptoCoin usdCoin = new("USD");

		private static CryptoCoin btcCoin = new("BTC");
        private static CryptoCoin ethCoin = new("ETH");
        private static CryptoCoin solCoin = new("SOL");
        private static CryptoCoin dotCoin = new("DOT");
        private static CryptoCoin adaCoin = new("ADA");
        private static CryptoCoin maticCoin = new("MATIC");
        private static CryptoCoin nearCoin = new("NEAR");
        private static CryptoCoin grtCoin = new("GRT");
        private static CryptoCoin ftmCoin = new("FTM");
        private static CryptoCoin algoCoin = new("ALGO");
        private static CryptoCoin icpCoin = new("ICP");
        private static CryptoCoin fetCoin = new("FET");
        private static CryptoCoin rndrCoin = new("RNDR");
        private static CryptoCoin aptCoin = new("APT");
        private static CryptoCoin xemCoin = new("XEM");
        private static CryptoCoin rplCoin = new("RPL");
        private static CryptoCoin qtumCoin = new("QTUM");
        private static CryptoCoin magicCoin = new("MAGIC");
        private static CryptoCoin blurCoin = new("BLUR");
        private static CryptoCoin oceanCoin = new("OCEAN");
        private static CryptoCoin stgCoin = new("STG");
        private static CryptoCoin gtcCoin = new("GTC");

        static async Task Main(string[] args)
		{
            var coins = new List<CryptoCoin> 
			{	
				btcCoin, ethCoin, solCoin, dotCoin, adaCoin, 
				maticCoin, nearCoin, grtCoin, ftmCoin, algoCoin, 
				icpCoin, fetCoin, rndrCoin, aptCoin, xemCoin, rplCoin,
                qtumCoin, magicCoin, blurCoin, oceanCoin, stgCoin, gtcCoin,
			};

            var exchanges = new List<Exchange>
            {
                new BinanceExchange(coins, new ExchangeTickersInfo("", CaseType.Uppercase, usdtCoin))
                    .RemoveCoins(new List<CryptoCoin>() { blurCoin }),
                
                new KucoinExchange(coins, new ExchangeTickersInfo("-", CaseType.Uppercase, usdtCoin))
                    .RemoveCoins(new List<CryptoCoin>() { qtumCoin }),
                
                new HuobiExchange(coins, new ExchangeTickersInfo("", CaseType.Lowercase, usdtCoin))
                    .RemoveCoins(new List<CryptoCoin>() { gtcCoin }),
                
                new GateioExchange(coins, new ExchangeTickersInfo("_", CaseType.Uppercase, usdtCoin))
                    .RemoveCoins(new List<CryptoCoin>() { gtcCoin }),

                new OkxExchange(coins, new ExchangeTickersInfo("-", CaseType.Uppercase, usdtCoin))
                    .RemoveCoins(new List < CryptoCoin >() { fetCoin, rndrCoin, rplCoin, oceanCoin, stgCoin, gtcCoin }),

                new BitmartExchange(coins, new ExchangeTickersInfo("_", CaseType.Uppercase, usdtCoin)),

                new BitstampExchange(coins, new ExchangeTickersInfo("/", CaseType.Uppercase, usdCoin))
                    .RemoveCoins(new List<CryptoCoin>() { icpCoin, aptCoin, xemCoin, rplCoin, qtumCoin, oceanCoin, blurCoin, magicCoin, stgCoin, gtcCoin }),

                new BitfinexExchange(coins, new ExchangeTickersInfo("", CaseType.Uppercase, usdCoin, "t"))
                    .RemoveCoins(new List<CryptoCoin>() { maticCoin, nearCoin, algoCoin, rndrCoin, xemCoin, rplCoin, qtumCoin, magicCoin, blurCoin, oceanCoin, gtcCoin })
            };

			var arbitrageChains = GetArbitrageChains(coins, exchanges).ToList();

			ThreadPool.QueueUserWorkItem(async (object obj) => await ArbitrageChainsFinder(exchanges, arbitrageChains));
			
			Console.ReadKey();
		}

		private static async Task ArbitrageChainsFinder(List<Exchange> exchanges, List<ArbitrageChainInfo> arbitrageChains)
		{
            while (true)
            {
                foreach (var exchange in exchanges)
                    await exchange.UpdateCoinPrices();

				var filteredChains = arbitrageChains
					.Where(c => c.FromExchange.HasCoin(c.Coin) && c.ToExchange.HasCoin(c.Coin));

				var topChains = filteredChains.OrderByDescending(c => c.Divergence).Take(10);

                foreach (var topChain in topChains)
                {
                    Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss.fff}]: " +
                            $"Diff = {topChain.Difference}, " +
                            $"Div = {topChain.Divergence}, " +
                            $"{topChain.Coin.Name} " +
                            $"({topChain.FromExchange.Name}-{topChain.ToExchange.Name})");

                    await File.AppendAllTextAsync("Chains.txt", 
                        $"{DateTime.UtcNow:HH:mm:ss.fff};" +
                        $"{topChain.Coin.Name};" +
                        $"{topChain.FromExchange.Name};" +
                        $"{topChain.ToExchange.Name};" +
                        $"{topChain.FromExchange.GetCoinPrice(topChain.Coin)};" +
                        $"{topChain.ToExchange.GetCoinPrice(topChain.Coin)};" +
                        $"{topChain.Difference};" +
                        $"{topChain.Divergence};" +
                        $"{Environment.NewLine}");
                }

                Console.WriteLine(Environment.NewLine);
            }
        }

		private static IEnumerable<ArbitrageChainInfo> GetArbitrageChains(List<CryptoCoin> coins, List<Exchange> exchanges)
		{
			var exchangesCombinations = GetExchangesCombinations(exchanges);

			foreach (var coin in coins)
			{
				foreach (var exchangesCombination in exchangesCombinations)
				{
					yield return new ArbitrageChainInfo(coin, exchangesCombination.Item1, exchangesCombination.Item2);
				}
			}
		}
		
		private static IEnumerable<Tuple<Exchange, Exchange>> GetExchangesCombinations(List<Exchange> exchanges)
		{
			for (int i = 0; i < exchanges.Count; i++)
			{
				for (int j = i + 1; j < exchanges.Count; j++)
				{
					yield return Tuple.Create(exchanges[i], exchanges[j]);
				}
			}
		}
	}
}

