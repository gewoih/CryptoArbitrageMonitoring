using CryptoArbitrageMonitoring.Models.Enums;

namespace CryptoArbitrageMonitoring.Models
{
    public sealed class ExchangeTickersInfo
    {
        public readonly string Separator;
        public readonly CaseType CaseType;
        public readonly CryptoCoin SecondCoin;
        
        public ExchangeTickersInfo(string separator, CaseType caseType, CryptoCoin secondCoin)
        {
            Separator = separator;
            CaseType = caseType;
            SecondCoin = secondCoin;
        }
    }
}
