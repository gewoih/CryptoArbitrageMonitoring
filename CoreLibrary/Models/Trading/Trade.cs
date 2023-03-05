namespace CoreLibrary.Models.Trading
{
    public sealed class Trade
    {
        public decimal EntryPrice { get; private set; }
        public decimal ExitPrice { get; private set; }
        public DateTime EntryDateTime { get; private set; }
        public DateTime ExitDateTime { get; private set; }
        public TradeType Type { get; private set; }
        public decimal Profit
        {
            get
            {
                if (IsClosed)
                    return GetEstimatedProfitForExitPrice(ExitPrice);

                return 0;
            }

        }
        public TimeSpan TimeInTrade => ExitDateTime - EntryDateTime;
        public bool IsClosed => ExitDateTime != DateTime.MinValue;

        public void Open(decimal entryPrice, TradeType type)
        {
            EntryPrice = entryPrice;
            Type = type;
            EntryDateTime = DateTime.UtcNow;
        }

        public void Close(decimal exitPrice)
        {
            ExitPrice = exitPrice;
            ExitDateTime = DateTime.UtcNow;
        }

        public decimal GetEstimatedProfitForExitPrice(decimal exitPrice)
        {
            if (Type == TradeType.Long)
                return exitPrice - EntryPrice;
            else if (Type == TradeType.Short)
                return EntryPrice - exitPrice;

            return 0;
        }
    }
}
