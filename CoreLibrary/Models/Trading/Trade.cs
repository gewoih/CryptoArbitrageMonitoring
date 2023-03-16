using CoreLibrary.Models.Enums;

namespace CoreLibrary.Models.Trading
{
    public sealed class Trade
    {
        public long Amount { get; set; }
        public decimal EntryPrice { get; private set; }
        public decimal ExitPrice { get; private set; }
        public DateTime EntryDateTime { get; private set; }
        public DateTime ExitDateTime { get; private set; }
        public TradeAction Type { get; private set; }
        public decimal Profit
        {
            get
            {
                if (IsClosed)
                    return GetEstimatedProfitForExitPrice(ExitPrice);

                return 0;
            }
        }
        public TimeSpan TimeInTrade => ExitDateTime == DateTime.MinValue ? DateTime.UtcNow - EntryDateTime : ExitDateTime - EntryDateTime;
        public bool IsClosed => ExitDateTime != DateTime.MinValue;

        public void Open(decimal entryPrice, long amount, TradeAction type)
        {
            EntryPrice = entryPrice;
            Amount = amount;
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
            if (Type == TradeAction.Long)
                return exitPrice - EntryPrice;
            else if (Type == TradeAction.Short)
                return EntryPrice - exitPrice;

            return 0;
        }
    }
}
