namespace CryptoArbitrageMonitoring.Models
{
    public sealed class ArbitrageChainInfo
    {
        public readonly Tuple<Exchange, decimal> MainExchangeInfo;
        public readonly Tuple<Exchange, decimal> SecondaryExchangeInfo;
        public decimal MainExchangePrice => MainExchangeInfo.Item2;
        public decimal SecondaryExchangePrice => SecondaryExchangeInfo.Item2;
        public decimal Difference => Math.Round(MainExchangePrice - SecondaryExchangePrice, 4);
        public decimal Divergence => Math.Round(MainExchangePrice / SecondaryExchangePrice * 100 - 100, 6);
        
        public ArbitrageChainInfo(Tuple<Exchange, decimal> mainExchangeInfo, Tuple<Exchange, decimal> secondaryExchangeInfo)
        {
            MainExchangeInfo = mainExchangeInfo;
            SecondaryExchangeInfo = secondaryExchangeInfo;
        }
    }
}
