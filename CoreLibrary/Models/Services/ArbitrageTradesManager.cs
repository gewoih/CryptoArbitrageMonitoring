using CoreLibrary.Models.Enums;
using CoreLibrary.Models.Trading;
using System.Collections.Concurrent;

namespace CoreLibrary.Models.Services
{
    public sealed class ArbitrageTradesManager
    {
        public event Action<ArbitrageTrade>? OnTradeOpened;
        public event Action<ArbitrageTrade>? OnTradeClosed;
        private readonly ConcurrentBag<ArbitrageTrade> _trades;
        private readonly int _minimumSecondsInTrade;
        private readonly decimal _stopLoss;
        private readonly decimal _takeProfit;
        private readonly decimal _amountPerTrade;

        public ArbitrageTradesManager(int minimumSecondsInTrade, decimal takeProfit, decimal amountPerTrade, decimal stopLoss)
        {
            _trades = new();
            StartClosingPositions();
            _minimumSecondsInTrade = minimumSecondsInTrade;
            _stopLoss = stopLoss;
            _takeProfit = takeProfit;
            _amountPerTrade = amountPerTrade;
        }

        public ArbitrageTrade TryOpenPositionByArbitrageChain(ArbitrageChain arbitrageChain)
        {
            //Есть ли незакрытая сделка с такой цепочкой?
            if (_trades.Any(trade => trade.ArbitrageChain.Equals(arbitrageChain) && !trade.LongTrade.IsClosed && !trade.ShortTrade.IsClosed))
                return null;

            var longTradePrice = arbitrageChain.FromExchangeMarketData.GetAverageMarketPriceForAmount(_amountPerTrade, TradeAction.Long);
            var shortTradePrice = arbitrageChain.ToExchangeMarketData.GetAverageMarketPriceForAmount(_amountPerTrade, TradeAction.Short);

            if (longTradePrice == 0 || shortTradePrice == 0)
                return null;

            var longAmount = (int)(_amountPerTrade / longTradePrice);
            var shortAmount = (int)(_amountPerTrade / shortTradePrice);

            var longTrade = new Trade();
            var shortTrade = new Trade();
            longTrade.Open(longTradePrice, longAmount, TradeAction.Long);
            shortTrade.Open(shortTradePrice, shortAmount, TradeAction.Short);

            var newArbitrageTrade = new ArbitrageTrade(arbitrageChain, longTrade, shortTrade);
            _trades.Add(newArbitrageTrade);

            OnTradeOpened?.Invoke(newArbitrageTrade);

            return newArbitrageTrade;
        }

        public bool IsAnyOpenedTradeForChain(ArbitrageChain chain)
        {
            return _trades.Any(trade => !trade.IsClosed && trade.ArbitrageChain.Equals(chain));
        }

        private void StartClosingPositions()
        {
            _ = Task.Run(async () =>
            {
                while (true)
                {
                    foreach (var trade in _trades.Where(trade => !trade.LongTrade.IsClosed && !trade.ShortTrade.IsClosed))
                    {
                        var longTradePrice = trade.ArbitrageChain.FromExchangeMarketData.GetAverageMarketPriceForAmount(_amountPerTrade, TradeAction.Short);
                        var shortTradePrice = trade.ArbitrageChain.ToExchangeMarketData.GetAverageMarketPriceForAmount(_amountPerTrade, TradeAction.Long);
                        var estimatedProfit = trade.GetEstimatedProfit(longTradePrice, shortTradePrice);

                        if ((estimatedProfit <= -_stopLoss || estimatedProfit >= _takeProfit) &&
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
