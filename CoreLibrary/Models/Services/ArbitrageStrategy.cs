using CoreLibrary.Models.Exchanges.Base;
using CoreLibrary.Models.Trading;

namespace CoreLibrary.Models.Services
{
    public sealed class ArbitrageStrategy
    {
        private readonly ArbitrageFinder _arbitrageFinder;
        private readonly ArbitrageTradesManager _tradesManager;
        public readonly decimal MinimumTotalDivergence;
        public readonly int DivergencePeriod;
        public bool IsStarted { get; private set; }

        public ArbitrageStrategy(List<CryptoCoin> coins, List<Exchange> exchanges, decimal minimumTotalDivergence, int divergencePeriod, int minimumSecondsInTrade)
        {
            MinimumTotalDivergence = minimumTotalDivergence;
            DivergencePeriod = divergencePeriod;

            _arbitrageFinder = new ArbitrageFinder(coins, exchanges, divergencePeriod);
            _tradesManager = new ArbitrageTradesManager(minimumSecondsInTrade);
            
            _tradesManager.OnTradeOpened += ArbitrageTradesManager_OnTradeOpened;
            _tradesManager.OnTradeClosed += ArbitrageTradesManager_OnTradeClosed;

        }

        public void Start()
        {
            if (!IsStarted)
            {
                Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss.fff}]: Starting arbitrage chains finder...");
                IsStarted = true;

                _ = Task.Run(() =>
                {
                    while (true)
                    {
                        try
                        {
                            var topChains = _arbitrageFinder.GetUpdatedChains(MinimumTotalDivergence);

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
        }

        private void ArbitrageTradesManager_OnTradeOpened(ArbitrageTrade trade)
        {
            Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss.fff}]: OPENED TRADE {trade} {Environment.NewLine}");
        }

        private void ArbitrageTradesManager_OnTradeClosed(ArbitrageTrade trade)
        {
            Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss.fff}]: CLOSED TRADE {trade} {Environment.NewLine}");

            File.AppendAllText($"Trades [{MinimumTotalDivergence} {DivergencePeriod}].txt",
                $"{trade.LongTrade.EntryDateTime};" +
                $"{trade.LongTrade.ExitDateTime};" +
                $"{trade.TimeInTrade.TotalSeconds};" +
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
