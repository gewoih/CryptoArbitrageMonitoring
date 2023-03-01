namespace CryptoArbitrageMonitoring.Extensions
{
    public static class DecimalExtensions
    {
        public static decimal Normalize(this decimal value)
        {
            return value / 1.00000000000000000000000m;
        }
    }
}
