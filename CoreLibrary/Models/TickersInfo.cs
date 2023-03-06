using CoreLibrary.Models.Enums;

namespace CoreLibrary.Models
{
    public sealed class TickersInfo
    {
        public readonly string Separator;
        public readonly CaseType CaseType;
        public readonly CryptoCoin SecondCoin;
        public readonly string Prefix;

        public TickersInfo(string separator, CaseType caseType, CryptoCoin secondCoin, string prefix = "")
        {
            Separator = separator;
            CaseType = caseType;
            SecondCoin = secondCoin;
            Prefix = prefix;
        }
    }
}
