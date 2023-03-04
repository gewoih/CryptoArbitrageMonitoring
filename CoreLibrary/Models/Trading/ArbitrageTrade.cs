namespace CoreLibrary.Models.Trading
{
    public sealed class ArbitrageTrade
    {
        public readonly ArbitrageChainInfo ArbitrageChain;
        public readonly Trade LongTrade;
        public readonly Trade ShortTrade;
        public decimal Profit => LongTrade.Profit + ShortTrade.Profit;
        
        public ArbitrageTrade(ArbitrageChainInfo arbitrageChain, Trade longTrade, Trade shortTrade)
        {
            ArbitrageChain = arbitrageChain;
            LongTrade = longTrade;
            ShortTrade = shortTrade;
        }
    }
}
