namespace CryptoArbitrageMonitoring.Models
{
    public sealed class CryptoCoin
    {
        public string Name { get; }
        
        public CryptoCoin(string name)
        {
            Name = name;
        }

        public override bool Equals(object? obj)
        {
            return obj is CryptoCoin coin &&
                   Name == coin.Name;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name);
        }
    }
}
