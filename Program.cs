using CryptoArbitrageMonitoring.Extensions;
using CryptoArbitrageMonitoring.Models;
using CryptoArbitrageMonitoring.Models.Exchanges;
using CryptoArbitrageMonitoring.Models.Exchanges.Base;
using CryptoArbitrageMonitoring.Utils;

namespace CryptoArbitrageMonitoring
{
    internal class Program
    {
        private static decimal minimumBidAskSpread = Settings1.Default.MinBidAskSpread;
        private static decimal exchangeComissionDivergence = Settings1.Default.ExchangeComissionDivergence;
        private static decimal minimumProfitDivergence = Settings1.Default.MinProfitDivergence;

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

            //Запускаем и ждем 1-е обновление (чтобы все биржи были заполнены)
            await StartUpdatingExchangesMarketData(exchanges);
            await Task.Delay(5000);

            var coins = CoinsUtils.GetCoins();
            var arbitrageChains = GetArbitrageChains(coins, exchanges).ToList();
            await ArbitrageChainsFinder(arbitrageChains);
        }

        private static async Task StartUpdatingExchangesMarketData(List<Exchange> exchanges)
        {
            foreach (var exchange in exchanges)
            {
                ThreadPool.QueueUserWorkItem(async (obj) =>
                {
                    while (true)
                    {
                        try
                        {
                            await exchange.UpdateCoinPrices();
                        }
                        catch (ArgumentNullException ex)
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
                    .Where(c => c.FromExchangeMarketData.Spread < minimumBidAskSpread)
                    .Where(c => c.ToExchangeMarketData.Spread < minimumBidAskSpread)
                    .Where(c => c.Difference != 0)
                    .Where(c => c.Divergence >= exchangeComissionDivergence + minimumProfitDivergence)
                    .OrderByDescending(c => c.Divergence);

                foreach (var topChain in topChains)
                {
                    var firstExchangeBid = topChain.FromExchangeMarketData.Bid.Normalize();
                    var firstExchangeAsk = topChain.FromExchangeMarketData.Ask.Normalize();
                    var firstExchangeLast = topChain.FromExchangeMarketData.Last.Normalize();
                    var firstExchangeSpread = topChain.FromExchangeMarketData.Spread.Normalize();

                    var secondExchangeBid = topChain.ToExchangeMarketData.Bid.Normalize();
                    var secondExchangeAsk = topChain.ToExchangeMarketData.Ask.Normalize();
                    var secondExchangeLast = topChain.ToExchangeMarketData.Last.Normalize();
                    var secondExchangeSpread = topChain.ToExchangeMarketData.Spread.Normalize();

                    Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss.fff}]: " +
                        $"(B:{firstExchangeBid}; A:{firstExchangeAsk}; L:{firstExchangeLast}; S:{firstExchangeSpread}%); " +
                        $"(B:{secondExchangeBid}; A:{secondExchangeAsk}; L:{secondExchangeLast}; S:{secondExchangeSpread}%); " +
                        $"{topChain.Difference.Normalize()}; " +
                        $"{topChain.Divergence.Normalize()}%; " +
                        $"{topChain.Coin.Name}" +
                        $"({topChain.FromExchange.Name}-{topChain.ToExchange.Name})");
                }

                if (topChains.Any())
                {
                    Console.WriteLine(Environment.NewLine);
                    await Task.Delay(100);
                }
            }
        }

        private static IEnumerable<ArbitrageChainInfo> GetArbitrageChains(List<CryptoCoin> coins, List<Exchange> exchanges)
        {
            var exchangesCombinations = GetExchangesCombinations(exchanges);

            foreach (var coin in coins)
            {
                foreach (var exchangesCombination in exchangesCombinations)
                {
                    if (exchangesCombination.Item1.HasCoin(coin) && exchangesCombination.Item2.HasCoin(coin))
                        yield return new ArbitrageChainInfo(coin, exchangesCombination.Item1, exchangesCombination.Item2);
                }
            }
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

