using CryptoArbitrageMonitoring.Models.Exchanges.Base;
using Newtonsoft.Json.Linq;

namespace CryptoArbitrageMonitoring.Models.Exchanges
{
    public sealed class HuobiExchange : Exchange
    {
        public override string Name => "Huobi";
        protected override string _baseApiEndpoint => "https://api.huobi.pro/market/tickers";

        public override async Task UpdateCoinPrices()
        {
            using var httpClient = new HttpClient();

            var result = await httpClient.GetAsync(_baseApiEndpoint);
            var pricesArray = JObject.Parse(await result.Content.ReadAsStringAsync());

            foreach (var ticker in CoinPrices.Keys.ToList())
            {
                var coinData = pricesArray["data"].First(p => p["symbol"].ToString() == CoinsToTickers[ticker]);
                var bid = Convert.ToDecimal(coinData["bid"]);
                var ask = Convert.ToDecimal(coinData["ask"]);

                CoinPrices[ticker] = (bid + ask) / 2;
            }
        }
    }
}
