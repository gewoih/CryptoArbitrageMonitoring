using CoreLibrary.Models.Trading;
using System.Collections.Concurrent;

namespace CoreLibrary.Models.Services
{
    public sealed class ArbitrageTradesManager
    {
        public event Action<ArbitrageTrade> OnTradeOpened;
        public event Action<ArbitrageTrade> OnTradeClosed;
        private readonly ConcurrentBag<ArbitrageTrade> _trades;
        private int _minimumSecondsInTrade;

        public ArbitrageTradesManager(int minimumSecondsInTrade)
        {
            _trades = new();
            StartClosingPositions();
            _minimumSecondsInTrade = minimumSecondsInTrade;
        }

        public ArbitrageTrade TryOpenPositionByArbitrageChain(ArbitrageChain arbitrageChain)
        {
            //Есть ли незакрытая сделка с такой цепочкой?
            if (_trades.Any(trade => trade.ArbitrageChain.Equals(arbitrageChain) && !trade.LongTrade.IsClosed && !trade.ShortTrade.IsClosed))
                return null;

            var longTradePrice = arbitrageChain.FromExchangeMarketData.GetLastTick().Ask;
            var shortTradePrice = arbitrageChain.ToExchangeMarketData.GetLastTick().Bid;
            
            var longTrade = new Trade();
            var shortTrade = new Trade();
            longTrade.Open(longTradePrice, TradeType.Long);
            shortTrade.Open(shortTradePrice, TradeType.Short);

            var newArbitrageTrade = new ArbitrageTrade(arbitrageChain, longTrade, shortTrade);
            _trades.Add(newArbitrageTrade);

            OnTradeOpened?.Invoke(newArbitrageTrade);

            return newArbitrageTrade;
        }

        private void StartClosingPositions()
        {
            _ = Task.Run(async () =>
            {
                while (true)
                {
                    foreach (var trade in _trades.Where(trade => !trade.LongTrade.IsClosed && !trade.ShortTrade.IsClosed))
                    {
                        var longTradePrice = trade.ArbitrageChain.FromExchangeMarketData.GetLastTick().Bid;
                        var shortTradePrice = trade.ArbitrageChain.ToExchangeMarketData.GetLastTick().Ask;
                        var estimatedProfit = trade.GetEstimatedProfit(longTradePrice, shortTradePrice);

                        if (trade.ArbitrageChain.GetCurrentDivergence() <= trade.EntryStandardDivergence &&
                            trade.TimeInTrade >= TimeSpan.FromSeconds(_minimumSecondsInTrade))
                        {
                            trade.LongTrade.Close(longTradePrice);
                            trade.ShortTrade.Close(shortTradePrice);

                            OnTradeClosed?.Invoke(trade);
                        }
                    }
                    await Task.Delay(1);
                }
            });
        }
    }
}
