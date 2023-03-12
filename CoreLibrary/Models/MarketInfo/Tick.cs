namespace CoreLibrary.Models.MarketInfo
{
    public sealed class Tick
    {
        public readonly int Number;
        public readonly decimal Price;
        public readonly long Ticks;
        
        public Tick(int number, decimal price, long ticks)
        {
            Number = number;
            Price = price;
            Ticks = ticks;
        }

        public Tick(decimal price, long ticks)
        {
            Price = price;
            Ticks = ticks;
        }
    }
}
