using CryptoArbitrageMonitoring.Models.Exchanges.Base;
using Newtonsoft.Json.Linq;

namespace CryptoArbitrageMonitoring.Models.Exchanges
{
    public sealed class BitfinexExchange : Exchange
    {
        public override string Name => "Bitfinex";
        protected override string _baseApiEndpoint => "https://api-pub.bitfinex.com/v2/tickers?symbols=ALL";
        
        public BitfinexExchange(List<CryptoCoin> coins, ExchangeTickersInfo tickersInfo) : base(coins, tickersInfo) { }

        public override async Task UpdateCoinPrices()
        {
            using var httpClient = new HttpClient();

            var result = await httpClient.GetAsync(_baseApiEndpoint);
            var pricesArray = JArray.Parse(await result.Content.ReadAsStringAsync());

            foreach (var coin in CoinPrices.Keys.ToList())
            {
                var coinData = pricesArray.First(p => p[0].ToString() == GetTickerByCoin(coin));
                var bid = Convert.ToDecimal(coinData[1]);
                var ask = Convert.ToDecimal(coinData[3]);

                CoinPrices[coin] = new MarketData { Bid = bid, Ask = ask };
            }
        }
    }
}
