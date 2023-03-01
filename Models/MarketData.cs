namespace CryptoArbitrageMonitoring.Models
{
    public sealed class MarketData
    {
        public decimal Bid { get; set; }
        public decimal Ask { get; set; }
        public decimal Last => Math.Round((Bid + Ask) / 2, 6);
        public decimal Spread => Bid != 0 && Ask != 0 ? Math.Round(Math.Abs(Bid / Ask * 100 - 100), 4) : 0;
    }
}
