using CoreLibrary.Extensions;

namespace CoreLibrary.Models.Trading
{
    public sealed class ArbitrageTrade
    {
        public readonly ArbitrageChain ArbitrageChain;
        public readonly Trade LongTrade;
        public readonly Trade ShortTrade;
        public readonly decimal EntryStandardDivergence;
        public decimal ProfitPercent => GetEstimatedProfit(LongTrade.ExitPrice, ShortTrade.ExitPrice);
        public TimeSpan TimeInTrade => (LongTrade.TimeInTrade + ShortTrade.TimeInTrade) / 2;
        public bool IsClosed => LongTrade.IsClosed && ShortTrade.IsClosed;
        public decimal Comission => (LongTrade.EntryPrice + LongTrade.ExitPrice + ShortTrade.EntryPrice + ShortTrade.ExitPrice) / 4 * 0.008m;
        
        public ArbitrageTrade(ArbitrageChain arbitrageChain, Trade longTrade, Trade shortTrade)
        {
            ArbitrageChain = arbitrageChain;
            LongTrade = longTrade;
            ShortTrade = shortTrade;
            EntryStandardDivergence = arbitrageChain.GetStandardDivergence();
        }

        public decimal GetEstimatedProfit(decimal longExitPrice, decimal shortExitPrice)
        {
            var longTradeEstimatedProfit = LongTrade.GetEstimatedProfitForExitPrice(longExitPrice);
            var shortTradeEstimatedProfit = ShortTrade.GetEstimatedProfitForExitPrice(shortExitPrice);

            return (longTradeEstimatedProfit + shortTradeEstimatedProfit) / (LongTrade.EntryPrice + ShortTrade.EntryPrice) * 100;
        }

        public override string? ToString()
        {
            var coinName = ArbitrageChain.Coin.Name;
            var fromExchangeName = ArbitrageChain.FromExchange.Name;
            var toExchangeName = ArbitrageChain.ToExchange.Name;

            if (IsClosed)
            {
                return
                    $"[{coinName}, {fromExchangeName}-{toExchangeName}] " +
                    $"[LONG {LongTrade.Profit.Normalize()}$, SHORT {ShortTrade.Profit.Normalize()}$]; " +
                    $"[TOTAL {Math.Round(ProfitPercent, 2)}%, Time: {TimeInTrade}];" +
                    $"{Environment.NewLine}" +
                    $"FOR CHAIN: {ArbitrageChain}";
            }
            else
            {
                return
                    $"[{coinName}, {fromExchangeName}-{toExchangeName}]" +
                    $"[LONG {LongTrade.EntryPrice.Normalize()}$, SHORT {ShortTrade.EntryPrice.Normalize()}$]; " +
                    $"{Environment.NewLine}" +
                    $"FOR CHAIN: {ArbitrageChain}";
            }
        }
    }
}
