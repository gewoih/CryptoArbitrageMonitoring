using CryptoArbitrageMonitoring.Models;
using CryptoArbitrageMonitoring.Models.Exchanges;
using CryptoArbitrageMonitoring.Models.Exchanges.Base;

namespace BinanceMonitoring
{
    internal class Program
	{
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
				new BinanceExchange()
					.WithCoins(new Dictionary<CryptoCoin, string> 
					{
						{ btcCoin, "BTCUSDT" },
						{ ethCoin, "ETHUSDT" },
						{ solCoin, "SOLUSDT" },
						{ dotCoin, "DOTUSDT" },
						{ adaCoin, "ADAUSDT" },
                        { maticCoin, "MATICUSDT" },
                        { nearCoin, "NEARUSDT" },
                        { grtCoin, "GRTUSDT" },
                        { ftmCoin, "FTMUSDT" },
                        { algoCoin, "ALGOUSDT" },
                        { icpCoin, "ICPUSDT" },
                        { fetCoin, "FETUSDT" },
                        { rndrCoin, "RNDRUSDT" },
                        { aptCoin, "APTUSDT" },
                        { xemCoin, "XEMUSDT" },
                        { rplCoin, "RPLUSDT" },

                        { qtumCoin, "QTUMUSDT" },
                        { magicCoin, "MAGICUSDT" },
                        //{ blurCoin, "BLURUSDT" },
                        { oceanCoin, "OCEANUSDT" },
                        { stgCoin, "STGUSDT" },
                        { gtcCoin, "GTCUSDT" },
                    }),

				new KucoinExchange()
					.WithCoins(new Dictionary<CryptoCoin, string>
					{
						{ btcCoin, "BTC-USDT" },
						{ ethCoin, "ETH-USDT" },
						{ solCoin, "SOL-USDT" },
						{ dotCoin, "DOT-USDT" },
                        { adaCoin, "ADA-USDT" },
                        { maticCoin, "MATIC-USDT" },
                        { nearCoin, "NEAR-USDT" },
                        { grtCoin, "GRT-USDT" },
                        { ftmCoin, "FTM-USDT" },
                        { algoCoin, "ALGO-USDT" },
                        { icpCoin, "ICP-USDT" },
                        { fetCoin, "FET-USDT" },
                        { rndrCoin, "RNDR-USDT" },
                        { aptCoin, "APT-USDT" },
                        { xemCoin, "XEM-USDT" },
                        { rplCoin, "RPL-USDT" },

                        //{ qtumCoin, "QTUM-USDT" },
                        { magicCoin, "MAGIC-USDT" },
                        { blurCoin, "BLUR-USDT" },
                        { oceanCoin, "OCEAN-USDT" },
                        { stgCoin, "STG-USDT" },
                        { gtcCoin, "GTC-USDT" },
                    }),

				new HuobiExchange()
					.WithCoins(new Dictionary<CryptoCoin, string>
					{
                        { btcCoin, "btcusdt" },
                        { ethCoin, "ethusdt" },
                        { solCoin, "solusdt" },
                        { dotCoin, "dotusdt" },
                        { adaCoin, "adausdt" },
                        { maticCoin, "maticusdt" },
                        { nearCoin, "nearusdt" },
                        { grtCoin, "grtusdt" },
                        { ftmCoin, "ftmusdt" },
                        { algoCoin, "algousdt" },
                        { icpCoin, "icpusdt" },
                        { fetCoin, "fetusdt" },
                        { rndrCoin, "rndrusdt" },
                        { aptCoin, "aptusdt" },
                        { xemCoin, "xemusdt" },
                        { rplCoin, "rplusdt" },

                        { qtumCoin, "qtumusdt" },
                        { magicCoin, "magicusdt" },
                        { blurCoin, "blurusdt" },
                        { oceanCoin, "oceanusdt" },
                        { stgCoin, "stgusdt" },
                        //{ gtcCoin, "gtcusdt" },
                    }),

				new GateioExchange()
					.WithCoins(new Dictionary<CryptoCoin, string>
					{
                        { btcCoin, "BTC_USDT" },
                        { ethCoin, "ETH_USDT" },
                        { solCoin, "SOL_USDT" },
                        { dotCoin, "DOT_USDT" },
                        { adaCoin, "ADA_USDT" },
                        { maticCoin, "MATIC_USDT" },
                        { nearCoin, "NEAR_USDT" },
                        { grtCoin, "GRT_USDT" },
                        { ftmCoin, "FTM_USDT" },
                        { algoCoin, "ALGO_USDT" },
                        { icpCoin, "ICP_USDT" },
                        { fetCoin, "FET_USDT" },
                        { rndrCoin, "RNDR_USDT" },
                        { aptCoin, "APT_USDT" },
                        { xemCoin, "XEM_USDT" },
                        { rplCoin, "RPL_USDT" },

                        { qtumCoin, "QTUM_USDT" },
                        { magicCoin, "MAGIC_USDT" },
                        { blurCoin, "BLUR_USDT" },
                        { oceanCoin, "OCEAN_USDT" },
                        { stgCoin, "STG_USDT" },
                        { gtcCoin, "GITCOIN_USDT" },
                    }),

				new OkxExchange()
					.WithCoins(new Dictionary<CryptoCoin, string>
					{
                        { btcCoin, "BTC-USDT" },
                        { ethCoin, "ETH-USDT" },
                        { solCoin, "SOL-USDT" },
                        { dotCoin, "DOT-USDT" },
                        { adaCoin, "ADA-USDT" },
                        { maticCoin, "MATIC-USDT" },
                        { nearCoin, "NEAR-USDT" },
                        { grtCoin, "GRT-USDT" },
                        { ftmCoin, "FTM-USDT" },
                        { algoCoin, "ALGO-USDT" },
                        { icpCoin, "ICP-USDT" },
                        //{ fetCoin, "FET-USDT" },
                        //{ rndrCoin, "RNDR-USDT" },
                        { aptCoin, "APT-USDT" },
                        { xemCoin, "XEM-USDT" },
                        //{ rplCoin, "RPL-USDT" }

                        { qtumCoin, "QTUM-USDT" },
                        { magicCoin, "MAGIC-USDT" },
                        { blurCoin, "BLUR-USDT" },
                        //{ oceanCoin, "OCEAN-USDT" },
                        //{ stgCoin, "STG-USDT" },
                        //{ gtcCoin, "GTC-USDT" },
                    }),

                //new BybitExchange()
                //    .WithCoins(new Dictionary<CryptoCoin, string>
                //    {
                //        { btcCoin, "BTCUSDT" },
                //        { ethCoin, "ETHUSDT" },
                //        { solCoin, "SOLUSDT" },
                //        { dotCoin, "DOTUSDT" },
                //        { adaCoin, "ADAUSDT" },
                //        //{ maticCoin, "MATICUSDT" },
                //        { nearCoin, "NEARUSDT" },
                //        //{ grtCoin, "GRTUSDT" },
                //        { ftmCoin, "FTMUSDT" },
                //        { algoCoin, "ALGOUSDT" },
                //        //{ icpCoin, "ICPUSDT" },
                //        //{ fetCoin, "FETUSDT" },
                //        //{ rndrCoin, "RNDRUSDT" },
                //        //{ aptCoin, "APTUSDT" },
                //        //{ xemCoin, "XEMUSDT" },
                //        //{ rplCoin, "RPLUSDT" }
                //    }),

                new BitmartExchange()
                    .WithCoins(new Dictionary<CryptoCoin, string>
                    {
                        { btcCoin, "BTC_USDT" },
                        { ethCoin, "ETH_USDT" },
                        { solCoin, "SOL_USDT" },
                        { dotCoin, "DOT_USDT" },
                        { adaCoin, "ADA_USDT" },
                        { maticCoin, "MATIC_USDT" },
                        { nearCoin, "NEAR_USDT" },
                        { grtCoin, "GRT_USDT" },
                        { ftmCoin, "FTM_USDT" },
                        { algoCoin, "ALGO_USDT" },
                        { icpCoin, "ICP_USDT" },
                        { fetCoin, "FET_USDT" },
                        { rndrCoin, "RNDR_USDT" },
                        { aptCoin, "APT_USDT" },
                        { xemCoin, "XEM_USDT" },
                        { rplCoin, "RPL_USDT" },

                        { qtumCoin, "QTUM_USDT" },
                        { magicCoin, "MAGIC_USDT" },
                        { blurCoin, "BLUR_USDT" },
                        { oceanCoin, "OCEAN_USDT" },
                        { stgCoin, "STG_USDT" },
                        { gtcCoin, "GTC_USDT" },
                    }),

                new BitstampExchange()
                    .WithCoins(new Dictionary<CryptoCoin, string>
                    {
                        { btcCoin, "BTC/USD" },
                        { ethCoin, "ETH/USDT" },
                        { solCoin, "SOL/USD" },
                        { dotCoin, "DOT/USD" },
                        { adaCoin, "ADA/USD" },
                        { maticCoin, "MATIC/USD" },
                        { nearCoin, "NEAR/USD" },
                        { grtCoin, "GRT/USD" },
                        { ftmCoin, "FTM/USD" },
                        { algoCoin, "ALGO/USD" },
                        //{ icpCoin, "ICP/USD" },
                        { fetCoin, "FET/USD" },
                        { rndrCoin, "RNDR/USD" },
                        //{ aptCoin, "APT/USD" },
                        //{ xemCoin, "XEM/USD" },
                        //{ rplCoin, "RPL/USD" },

                        //{ qtumCoin, "QTUM/USD" },
                        //{ magicCoin, "MAGIC/USD" },
                        //{ blurCoin, "BLUR/USDT" },
                        //{ oceanCoin, "OCEAN/USDT" },
                        //{ stgCoin, "STG/USDT" },
                        //{ gtcCoin, "GTC/USDT" },
                    })
            };

			var arbitrageChains = GetArbitrageChains(coins, exchanges).ToList();

			ThreadPool.QueueUserWorkItem(async (object obj) => await ArbitrageChainsFinder(exchanges, arbitrageChains));
			
			Console.ReadKey();

			////var bitfinex = new Exchange("Bitfinex", "https://api-pub.bitfinex.com/v2/ticker/")
			////	.AddTicker("tDOTUSD");
		}

		private static async Task ArbitrageChainsFinder(List<Exchange> exchanges, List<ArbitrageChainInfo> arbitrageChains)
		{
            while (true)
            {
                if (DateTime.Now.Second != 0)
                {
                    await Task.Delay(10);
                    continue;
                }

                var tasks = new List<Task>();
                foreach (var exchange in exchanges)
                {
                    tasks.Add(exchange.UpdateCoinPrices());
                }
                await Task.WhenAll(tasks);

				var filteredChains = arbitrageChains
					.Where(c => c.FromExchange.HasCoin(c.Coin) && c.ToExchange.HasCoin(c.Coin));

				var topChains = filteredChains.OrderByDescending(c => c.Divergence);

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

