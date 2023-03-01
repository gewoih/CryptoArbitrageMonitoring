namespace CryptoArbitrageMonitoring.Models
{
    public sealed class MarketData
    {
        public decimal Bid { get; set; }
        public decimal Ask { get; set; }
        public decimal Last => (Bid + Ask) / 2;
        public decimal Divergence => Bid != 0 && Ask != 0 ? Math.Abs(Bid / Ask * 100 - 100) : 0;
    }
}
