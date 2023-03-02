using CoreLibrary.Models;
using CoreLibrary.Models.Exchanges;
using CoreLibrary.Models.Exchanges.Base;
using CoreLibrary.Utils;

namespace CryptoArbitrageMonitoringApp
{
    internal class Program
    {
        private static readonly decimal minimumBidAskSpread = Settings1.Default.MinBidAskSpread;
        private static readonly decimal exchangeComissionDivergence = Settings1.Default.ExchangeComissionDivergence;
        private static readonly decimal minimumProfitDivergence = Settings1.Default.MinProfitDivergence;

        static async Task Main(string[] args)
        {
            //using var httpClient = new HttpClient();

            //using var result = await httpClient.GetAsync("https://api.binance.com/api/v3/ticker/bookTicker");
            //var pricesArray = JArray.Parse(await result.Content.ReadAsStringAsync());

            //var coins = pricesArray
            //                .Where(p => p["symbol"].ToString().EndsWith("USDT"))
            //                .Select(p => p["symbol"].ToString().Replace("USDT", ""));

            //foreach (var coin in coins)
            //{
            //    Console.WriteLine(coin);
            //}

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

            //Запускаем и ждем 1-е обновление (чтобы все биржи были заполнены)
            StartUpdatingExchangesMarketData(exchanges);
            await Task.Delay(5000);

            var coins = CoinsUtils.GetCoins();
            var arbitrageChains = GetArbitrageChains(coins, exchanges).ToList();
            await ArbitrageChainsFinder(arbitrageChains);
        }

        private static void StartUpdatingExchangesMarketData(List<Exchange> exchanges)
        {
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
                        catch
                        {
                        }
                    }
                });
            }
        }

        private static async Task ArbitrageChainsFinder(List<ArbitrageChainInfo> arbitrageChains)
        {
            while (true)
            {
                var topChains = arbitrageChains
                    .Where(c => c.FromExchangeMarketData.Spread < minimumBidAskSpread &&
                        c.ToExchangeMarketData.Spread < minimumBidAskSpread &&
                        c.Difference != 0 &&
                        c.Divergence >= exchangeComissionDivergence + minimumProfitDivergence)
                    .OrderByDescending(c => c.Divergence)
                    .Take(10);

                foreach (var topChain in topChains)
                {
                    Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss.fff}]: {topChain}");
                }

                if (topChains.Any())
                {
                    Console.WriteLine();
                    await Task.Delay(200);
                }
            }
        }

        private static IEnumerable<ArbitrageChainInfo> GetArbitrageChains(List<CryptoCoin> coins, List<Exchange> exchanges)
        {
            return coins
                .SelectMany(coin => GetExchangesCombinations(exchanges)
                    .Where(exchangePair => exchangePair.Item1.HasCoin(coin) && exchangePair.Item2.HasCoin(coin))
                    .Select(exchangePair => new ArbitrageChainInfo(coin, exchangePair.Item1, exchangePair.Item2)));
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

