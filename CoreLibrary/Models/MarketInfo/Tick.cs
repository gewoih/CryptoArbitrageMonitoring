namespace CoreLibrary.Models.MarketInfo
{
    public sealed class Tick
    {
        public readonly decimal Bid;
        public readonly decimal Ask;
        public readonly decimal Last;
        public readonly DateTime Time;
        public decimal Spread => Bid != 0 && Ask != 0 ? Math.Round(Math.Abs(Bid / Ask * 100 - 100), 4) : 0;
        
        public Tick(decimal bid, decimal ask, decimal last)
        {
            Bid = bid;
            Ask = ask;

            if (last == 0)
                Last = (bid + ask) / 2;
            else
                Last = last;
        }
    }
}
