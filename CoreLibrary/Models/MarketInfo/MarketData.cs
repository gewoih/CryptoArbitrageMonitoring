namespace CoreLibrary.Models.MarketInfo
{
    public sealed class MarketData
    {
        public readonly List<Tick> Ticks = new();
        public DateTime LastUpdate { get; private set; }

        public void AddTick(decimal bid, decimal ask, decimal last)
        {
            if (Ticks.Any() && last == Ticks.Last().Last)
                return;

            Ticks.Add(new(bid, ask, last));
            LastUpdate = DateTime.UtcNow;
        }

        public decimal GetSMA(int period)
        {
            if (period <= 0)
                throw new ArgumentOutOfRangeException(nameof(period));

            if (Ticks.Count < period)
                return 0;

            return Ticks
                    .TakeLast(period)
                    .Average(tick => tick.Last);
        }

        public Tick GetLastTick()
        {
            var lastTick = Ticks.LastOrDefault();

            if (lastTick != null)
                return lastTick;

            return new(0, 0, 0);
        }
    }
}
