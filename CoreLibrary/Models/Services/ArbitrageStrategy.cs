using CoreLibrary.Models.Exchanges.Base;
using CoreLibrary.Models.Trading;

namespace CoreLibrary.Models.Services
{
    public sealed class ArbitrageStrategy
    {
        private readonly ArbitrageFinder _arbitrageFinder;
        private readonly ArbitrageTradesManager _tradesManager;
        private readonly decimal _minimumTotalDivergence;
        private readonly int _divergencePeriod;
        private readonly int _minimumSecondsInTrade;
        private readonly decimal _takeProfit;
        private readonly decimal _stopLoss;

        public ArbitrageStrategy(List<CryptoCoin> coins, List<Exchange> exchanges, decimal minimumTotalDivergence, int divergencePeriod, 
            int minimumSecondsInTrade, decimal takeProfit, decimal stopLoss, decimal amountPerTrade)
        {
            _minimumTotalDivergence = minimumTotalDivergence;
            _divergencePeriod = divergencePeriod;
            _minimumSecondsInTrade = minimumSecondsInTrade;
            _takeProfit = takeProfit;
            _stopLoss = stopLoss;

            _arbitrageFinder = new ArbitrageFinder(coins, exchanges, divergencePeriod, amountPerTrade);
            _tradesManager = new ArbitrageTradesManager(minimumSecondsInTrade, takeProfit, amountPerTrade, stopLoss);
            
            _tradesManager.OnTradeOpened += ArbitrageTradesManager_OnTradeOpened;
            _tradesManager.OnTradeClosed += ArbitrageTradesManager_OnTradeClosed;
        }

        public void Start()
        {
            Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss.fff}]: Starting arbitrage chains finder...");

            _ = Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        var topChains = _arbitrageFinder.GetUpdatedChains(_minimumTotalDivergence);

                        foreach (var topChain in topChains)
                        {
                            var newTrade = _tradesManager.TryOpenPositionByArbitrageChain(topChain);
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }
            });
        }

        private void ArbitrageTradesManager_OnTradeOpened(ArbitrageTrade trade)
        {
            Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss.fff}]: OPENED TRADE {trade} {Environment.NewLine}");
        }

        private void ArbitrageTradesManager_OnTradeClosed(ArbitrageTrade trade)
        {
            Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss.fff}]: CLOSED TRADE {trade} {Environment.NewLine}");

            var arbitrageTradeProfitWithComission = (trade.LongTrade.Profit + trade.ShortTrade.Profit - trade.Comission) / 
                                                    (trade.LongTrade.EntryPrice + trade.ShortTrade.EntryPrice) 
                                                    * 100;

			File.AppendAllText($"Trades [" +
                    $"{_minimumTotalDivergence.ToString().Replace(".", ",")} " +
                    $"{_divergencePeriod} " +
                    $"{_minimumSecondsInTrade} " +
                    $"{_takeProfit.ToString().Replace(".", ",")} " +
                    $"{_stopLoss.ToString().Replace(".", ",")}].txt",
                $"{trade.LongTrade.EntryDateTime};" +
                $"{trade.LongTrade.ExitDateTime};" +
                $"{(int)trade.TimeInTrade.TotalSeconds};" +
                $"{trade.ArbitrageChain.FromExchange.Name};" +
                $"{trade.ArbitrageChain.ToExchange.Name};" +
                $"{trade.ArbitrageChain.Coin.Name};" +
                $"{trade.LongTrade.EntryPrice};" +
                $"{trade.ShortTrade.EntryPrice};" +
                $"{trade.LongTrade.ExitPrice};" +
                $"{trade.ShortTrade.ExitPrice};" +
                $"{trade.LongTrade.Profit};" +
                $"{trade.ShortTrade.Profit};" +
                $"{trade.LongTrade.Profit + trade.ShortTrade.Profit};" +
                $"{trade.Comission};" +
                $"{arbitrageTradeProfitWithComission}" +
                $"{Environment.NewLine}");
        }
    }
}
