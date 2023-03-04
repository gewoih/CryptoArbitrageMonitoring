namespace CoreLibrary.Models.Trading
{
    public sealed class Trade
    {
        public decimal EntryPrice { get; private set; }
        public decimal ExitPrice { get; private set; }
        public decimal Profit => ExitPrice / EntryPrice * 100 - 100;
        public DateTime EntryDateTime { get; private set; }
        public DateTime ExitDateTime { get; private set; }
        public TimeSpan TimeInTrade => ExitDateTime - EntryDateTime;
        public bool IsClosed => ExitDateTime != DateTime.MinValue;

        public void Open(decimal entryPrice)
        {
            EntryPrice = entryPrice;
            EntryDateTime = DateTime.UtcNow;
        }

        public void Close(decimal exitPrice)
        {
            ExitPrice = exitPrice;
            ExitDateTime = DateTime.UtcNow;
        }
    }
}
