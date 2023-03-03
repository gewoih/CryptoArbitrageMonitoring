namespace CoreLibrary.Models.MarketInfo
{
    public sealed class Tick
    {
        public readonly decimal Bid;
        public readonly decimal Ask;
        public readonly DateTime Time;
        public decimal Last => Math.Round((Bid + Ask) / 2, 6);
        public decimal Spread => Bid != 0 && Ask != 0 ? Math.Round(Math.Abs(Bid / Ask * 100 - 100), 4) : 0;
        
        public Tick(decimal bid, decimal ask)
        {
            Bid = bid;
            Ask = ask;
        }
    }
}
