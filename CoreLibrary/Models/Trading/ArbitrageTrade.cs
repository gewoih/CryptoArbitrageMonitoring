namespace CoreLibrary.Models.Trading
{
    public sealed class ArbitrageTrade
    {
        public readonly ArbitrageChainInfo ArbitrageChain;
        public readonly Trade LongTrade;
        public readonly Trade ShortTrade;
        public readonly decimal EntryStandardDivergence;
        public decimal ProfitPercent => GetEstimatedProfit(LongTrade.ExitPrice, ShortTrade.ExitPrice);
        public TimeSpan TimeInTrade => (LongTrade.TimeInTrade + ShortTrade.TimeInTrade) / 2;
        
        public ArbitrageTrade(ArbitrageChainInfo arbitrageChain, Trade longTrade, Trade shortTrade)
        {
            ArbitrageChain = arbitrageChain;
            LongTrade = longTrade;
            ShortTrade = shortTrade;
            EntryStandardDivergence = ArbitrageChain.GetStandardDivergence();
        }

        public decimal GetEstimatedProfit(decimal longExitPrice, decimal shortExitPrice)
        {
            var longTradeEstimatedProfit = LongTrade.GetEstimatedProfitForExitPrice(longExitPrice);
            var shortTradeEstimatedProfit = ShortTrade.GetEstimatedProfitForExitPrice(shortExitPrice);

            return (longTradeEstimatedProfit + shortTradeEstimatedProfit) / (LongTrade.EntryPrice + ShortTrade.EntryPrice) * 100;
        }
    }
}
