namespace CryptoArbitrageMonitoring.Models
{
    public sealed class CryptoCoin
    {
        public string Name { get; }
        
        public CryptoCoin(string name)
        {
            Name = name;
        }
    }
}
