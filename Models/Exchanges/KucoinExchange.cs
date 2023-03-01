using CryptoArbitrageMonitoring.Models.Exchanges.Base;
using Newtonsoft.Json.Linq;

namespace CryptoArbitrageMonitoring.Models.Exchanges
{
    public sealed class KucoinExchange : Exchange
    {
        public override string Name => "Kucoin";
        protected override string _baseApiEndpoint => "https://api.kucoin.com/api/v1/market/allTickers";
        
        public KucoinExchange(List<CryptoCoin> coins, ExchangeTickersInfo tickersInfo) : base(coins, tickersInfo) { }

        public override async Task UpdateCoinPrices()
        {
            using var httpClient = new HttpClient();

            var result = await httpClient.GetAsync(_baseApiEndpoint);
            var prices = JObject.Parse(await result.Content.ReadAsStringAsync());

            foreach (var coin in CoinPrices.Keys.ToList())
            {
                var coinData = prices["data"]["ticker"].First(p => p["symbol"].ToString() == GetTickerByCoin(coin));
                var bid = Convert.ToDecimal(coinData["buy"]);
                var ask = Convert.ToDecimal(coinData["sell"]);

                CoinPrices[coin] = (bid + ask) / 2;
            }
        }
    }
}
