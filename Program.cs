using CryptoArbitrageMonitoring.Extensions;
using CryptoArbitrageMonitoring.Models;
using CryptoArbitrageMonitoring.Models.Enums;
using CryptoArbitrageMonitoring.Models.Exchanges;
using CryptoArbitrageMonitoring.Models.Exchanges.Base;

namespace CryptoArbitrageMonitoring
{
    internal class Program
    {
        private static decimal minimumBidAskSpread = Settings1.Default.MinBidAskSpread;
        private static decimal exchangeComissionDivergence = Settings1.Default.ExchangeComissionDivergence;
        private static decimal minimumProfitDivergence = Settings1.Default.MinProfitDivergence;


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
        private static CryptoCoin roseCoin = new("ROSE");
        private static CryptoCoin ankrCoin = new("ANKR");
        private static CryptoCoin kavaCoin = new("KAVA");
        private static CryptoCoin arCoin = new("AR");
        private static CryptoCoin ksmCoin = new("KSM");
        private static CryptoCoin rsrCoin = new("RSR");
        private static CryptoCoin achCoin = new("ACH");
        private static CryptoCoin slpCoin = new("SLP");
        private static CryptoCoin xrdCoin = new("XRD");
        private static CryptoCoin vgxCoin = new("VGX");

        static async Task Main(string[] args)
        {
            var coins = new List<CryptoCoin>
            {
                btcCoin, ethCoin, solCoin, dotCoin, adaCoin,
                maticCoin, nearCoin, grtCoin, ftmCoin, algoCoin,
                icpCoin, fetCoin, rndrCoin, aptCoin, xemCoin, rplCoin,
                qtumCoin, magicCoin, blurCoin, oceanCoin, stgCoin, gtcCoin,
                roseCoin, ankrCoin, kavaCoin, ksmCoin, rsrCoin, achCoin, slpCoin, xrdCoin, vgxCoin, arCoin
            };

            var exchanges = new List<Exchange>
            {
                new BinanceExchange(coins, new ExchangeTickersInfo("", CaseType.Uppercase, usdtCoin))
                    .RemoveCoins(new List<CryptoCoin>() { blurCoin, xrdCoin }),

                new KucoinExchange(coins, new ExchangeTickersInfo("-", CaseType.Uppercase, usdtCoin))
                    .RemoveCoins(new List<CryptoCoin>() { qtumCoin, xrdCoin, vgxCoin }),

                new HuobiExchange(coins, new ExchangeTickersInfo("", CaseType.Lowercase, usdtCoin))
                    .RemoveCoins(new List<CryptoCoin>() { gtcCoin, roseCoin, slpCoin, xrdCoin, vgxCoin }),

                new GateioExchange(coins, new ExchangeTickersInfo("_", CaseType.Uppercase, usdtCoin))
                    .RemoveCoins(new List<CryptoCoin>() { gtcCoin }),

                new OkxExchange(coins, new ExchangeTickersInfo("-", CaseType.Uppercase, usdtCoin))
                    .RemoveCoins(new List<CryptoCoin>() { fetCoin, rndrCoin, rplCoin, oceanCoin, stgCoin, gtcCoin, roseCoin,
                        ankrCoin, kavaCoin, achCoin, xrdCoin, vgxCoin }),

                new BitmartExchange(coins, new ExchangeTickersInfo("_", CaseType.Uppercase, usdtCoin))
                    .RemoveCoins(new List<CryptoCoin> { roseCoin, vgxCoin }),

                new BitstampExchange(coins, new ExchangeTickersInfo("/", CaseType.Uppercase, usdCoin))
                    .RemoveCoins(new List<CryptoCoin>() { icpCoin, aptCoin, xemCoin, rplCoin, qtumCoin, oceanCoin, blurCoin, magicCoin,
                        stgCoin, gtcCoin, roseCoin, ankrCoin, kavaCoin, ksmCoin, rsrCoin, achCoin, xrdCoin, vgxCoin, arCoin }),

                new BitfinexExchange(coins, new ExchangeTickersInfo("", CaseType.Uppercase, usdCoin, "t"))
                    .RemoveCoins(new List<CryptoCoin>() { maticCoin, nearCoin, algoCoin, rndrCoin, xemCoin, rplCoin, qtumCoin,
                        magicCoin, blurCoin, oceanCoin, gtcCoin, roseCoin, ankrCoin, kavaCoin, rsrCoin, achCoin, slpCoin, vgxCoin, arCoin })
            };

            var arbitrageChains = GetArbitrageChains(coins, exchanges).ToList();

            await ArbitrageChainsFinder(exchanges, arbitrageChains);

            Console.ReadKey();
        }

        private static async Task ArbitrageChainsFinder(List<Exchange> exchanges, List<ArbitrageChainInfo> arbitrageChains)
        {
            foreach (var exchange in exchanges)
            {
                ThreadPool.QueueUserWorkItem(async (obj) =>
                {
                    while (true)
                    {
                        await exchange.UpdateCoinPrices();
                    }
                });
            }

            while (true)
            {
                var filteredChains = arbitrageChains
                    .Where(c => c.FromExchange.HasCoin(c.Coin) && c.ToExchange.HasCoin(c.Coin));

                var topChains = filteredChains
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

                if (topChains.Count() > 1)
                    Console.WriteLine(Environment.NewLine);

                await Task.Delay(100);
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
                for (int j = 0; j < exchanges.Count; j++)
                {
                    if (j != i)
                        yield return Tuple.Create(exchanges[i], exchanges[j]);
                }
            }
        }
    }
}

