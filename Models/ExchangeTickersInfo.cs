using CryptoArbitrageMonitoring.Models.Enums;

namespace CryptoArbitrageMonitoring.Models
{
    public sealed class ExchangeTickersInfo
    {
        public readonly string Separator;
        public readonly CaseType CaseType;
        public readonly CryptoCoin SecondCoin;
        public readonly string Prefix;
        
        public ExchangeTickersInfo(string separator, CaseType caseType, CryptoCoin secondCoin, string prefix = "")
        {
            Separator = separator;
            CaseType = caseType;
            SecondCoin = secondCoin;
            Prefix = prefix;
        }
    }
}
