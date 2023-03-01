using CryptoArbitrageMonitoring.Models.Exchanges.Base;
using Newtonsoft.Json.Linq;

namespace CryptoArbitrageMonitoring.Models.Exchanges
{
    public sealed class BitmartExchange : Exchange
    {
        public override string Name => "Bitmart";
        protected override string _baseApiEndpoint => "https://api-cloud.bitmart.com/spot/v1/ticker";

        public override async Task UpdateCoinPrices()
        {
            using var httpClient = new HttpClient();

            var result = await httpClient.GetAsync(_baseApiEndpoint);
            var prices = JObject.Parse(await result.Content.ReadAsStringAsync());

            foreach (var ticker in CoinPrices.Keys.ToList())
            {
                try
                {
                    var coinData = prices["data"]["tickers"].First(p => p["symbol"].ToString() == CoinsToTickers[ticker]);
                    var bid = Convert.ToDecimal(coinData["best_bid"]);
                    var ask = Convert.ToDecimal(coinData["best_ask"]);

                    CoinPrices[ticker] = (bid + ask) / 2;
                }
                catch { }
            }
        }
    }
}
