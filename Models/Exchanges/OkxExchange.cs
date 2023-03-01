using CryptoArbitrageMonitoring.Models.Exchanges.Base;
using Newtonsoft.Json.Linq;

namespace CryptoArbitrageMonitoring.Models.Exchanges
{
    public sealed class OkxExchange : Exchange
    {
        public override string Name => "Okx";

        protected override string _baseApiEndpoint => "https://www.okx.com/api/v5/market/tickers?instType=SPOT";

        public override async Task UpdateCoinPrices()
        {
            using var httpClient = new HttpClient();

            var result = await httpClient.GetAsync(_baseApiEndpoint);
            var prices = JObject.Parse(await result.Content.ReadAsStringAsync());

            foreach (var ticker in CoinPrices.Keys.ToList())
            {
                var coinData = prices["data"].First(p => p["instId"].ToString() == CoinsToTickers[ticker]);
                var bid = Convert.ToDecimal(coinData["bidPx"]);
                var ask = Convert.ToDecimal(coinData["askPx"]);

                CoinPrices[ticker] = (bid + ask) / 2;
            }
        }
    }
}
