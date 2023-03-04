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
        private static readonly decimal exchangeComissionDivergence = Settings1.Default.ExchangeComissionDivergence;
        private static readonly decimal minimumProfitDivergence = Settings1.Default.MinProfitDivergence;
        private static readonly int divergencePeriod = Settings1.Default.DivergencePeriod;
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

            //Запускаем и ждем 1-е обновление (чтобы все биржи были заполнены)
            StartUpdatingExchangesMarketData(exchanges);
            await WaitForAllMarketDataLoaded(exchanges);

            var coins = CoinsUtils.GetCoins();
            var arbitrageChains = GetArbitrageChains(coins, exchanges).ToList();
            await ArbitrageChainsFinder(arbitrageChains);
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
                        catch
                        {
                            await Task.Delay(1000);
                            continue;
                        }
                    }
                });
            }
        }

        private static async Task ArbitrageChainsFinder(List<ArbitrageChainInfo> arbitrageChains)
        {
            Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss.fff}]: Starting arbitrage chains finder...");

            while (true)
            {
                try
                {
                    var topChains = arbitrageChains
                        .Where(chain =>
                            //chain.FromExchangeMarketData.GetLastTick().Spread < minimumBidAskSpread &&
                            //chain.ToExchangeMarketData.GetLastTick().Spread < minimumBidAskSpread &&
                            //chain.FromExchangeMarketData.GetLastTick().Last > 0.001m &&
                            //chain.ToExchangeMarketData.GetLastTick().Last > 0.001m &&
                            chain.GetCurrentDifference() != 0 &&
                            chain.GetTotalDivergence() != 0 &&
                            chain.GetTotalDivergence() >= exchangeComissionDivergence + minimumProfitDivergence &&
                            chain.FromExchangeMarketData.GetLastTick().Last < chain.ToExchangeMarketData.GetLastTick().Last &&
                            !Trades.Any(trade => trade.ArbitrageChain.Equals(chain) && !trade.LongTrade.IsClosed && !trade.ShortTrade.IsClosed))
                        .OrderByDescending(c => c.GetTotalDivergence());

                    foreach (var topChain in topChains)
                    {
                        Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss.fff}]: {topChain}" + Environment.NewLine);
                    }
                    
                    OpenPositionsByArbitrageChains(topChains);
                    ClosePositionsByArbitrageChains();

                    if (topChains.Any())
                    {
                        await Task.Delay(200);
                    }
                }
                catch
                {
                    continue;
                }
            }
        }

        private static void OpenPositionsByArbitrageChains(IEnumerable<ArbitrageChainInfo> arbitrageChain)
        {
            foreach (var chain in arbitrageChain)
            {
                //Есть ли незакрытая сделка с такой цепочкой?
                if (Trades.Any(trade => trade.ArbitrageChain.Equals(chain) && !trade.LongTrade.IsClosed && !trade.ShortTrade.IsClosed))
                    continue;

                var longTrade = new Trade();
                var shortTrade = new Trade();
                var newArbitrageTrade = new ArbitrageTrade(chain, longTrade, shortTrade);

                var longTradePrice = chain.FromExchangeMarketData.GetLastTick().Ask;
                var shortTradePrice = chain.ToExchangeMarketData.GetLastTick().Bid;

                longTrade.Open(longTradePrice);
                shortTrade.Open(shortTradePrice);

                Trades.Add(newArbitrageTrade);

                Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss.fff}]: [{newArbitrageTrade.ArbitrageChain.Coin.Name}] Arbitrage trade opened. " +
                    $"[LONG on exchange '{newArbitrageTrade.ArbitrageChain.FromExchange.Name}' on price {longTradePrice}$]; " +
                    $"[SHORT on exchange '{newArbitrageTrade.ArbitrageChain.ToExchange.Name}' on price {shortTradePrice}$]" +
                    $"{Environment.NewLine}");
            }
        }

        private static void ClosePositionsByArbitrageChains()
        {
            foreach (var trade in Trades.Where(trade => !trade.LongTrade.IsClosed && !trade.ShortTrade.IsClosed))
            {
                if (trade.ArbitrageChain.GetCurrentDivergence() < trade.ArbitrageChain.GetStandardDivergence())
                {
                    var longTradePrice = trade.ArbitrageChain.FromExchangeMarketData.GetLastTick().Bid;
                    var shortTradePrice = trade.ArbitrageChain.ToExchangeMarketData.GetLastTick().Ask;

                    trade.LongTrade.Close(longTradePrice);
                    trade.ShortTrade.Close(shortTradePrice);

                    var longTradeProfit = trade.LongTrade.Profit;
                    var shortTradeProfit = trade.ShortTrade.Profit;
                    var totalProfit = Math.Round(longTradeProfit + shortTradeProfit, 6).Normalize();
                    var timeInPosition = (trade.LongTrade.TimeInTrade + trade.ShortTrade.TimeInTrade) / 2;

                    Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss.fff}]: [{trade.ArbitrageChain.Coin.Name}] Arbitrage trade closed. " +
                        $"[Close LONG on exchange '{trade.ArbitrageChain.FromExchange.Name}' on price {longTradePrice}$, Profit = {longTradeProfit}%]; " +
                        $"[Close SHORT on exchange '{trade.ArbitrageChain.ToExchange.Name}' on price {shortTradePrice}$, Profit = {shortTradeProfit}%]; " +
                        $"[Total profit = {totalProfit}%, Time in position: {timeInPosition}]" +
                        $"{Environment.NewLine}");

                    File.AppendAllText("Trades.txt", $"" +
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
            }
        }

        private static IEnumerable<ArbitrageChainInfo> GetArbitrageChains(List<CryptoCoin> coins, List<Exchange> exchanges)
        {
            return coins
                .SelectMany(coin => GetExchangesCombinations(exchanges)
                    .Where(exchangePair => exchangePair.Item1.HasCoin(coin) && exchangePair.Item2.HasCoin(coin))
                    .Select(exchangePair => new ArbitrageChainInfo(coin, exchangePair.Item1, exchangePair.Item2, divergencePeriod)));
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

