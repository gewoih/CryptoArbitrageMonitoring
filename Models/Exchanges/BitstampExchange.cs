using CryptoArbitrageMonitoring.Models.Exchanges.Base;
using Newtonsoft.Json.Linq;

namespace CryptoArbitrageMonitoring.Models.Exchanges
{
    public sealed class BitstampExchange : Exchange
    {
        public override string Name => "Bitstamp";
        protected override string _baseApiEndpoint => "https://www.bitstamp.net/api/v2/ticker/";

        public override async Task UpdateCoinPrices()
        {
            using var httpClient = new HttpClient();

            var result = await httpClient.GetAsync(_baseApiEndpoint);
            var pricesArray = JArray.Parse(await result.Content.ReadAsStringAsync());

            foreach (var ticker in CoinPrices.Keys.ToList())
            {
                var coinData = pricesArray.First(p => p["pair"].ToString() == CoinsToTickers[ticker]);
                var bid = Convert.ToDecimal(coinData["bid"]);
                var ask = Convert.ToDecimal(coinData["ask"]);

                CoinPrices[ticker] = (bid + ask) / 2;
            }
        }
    }
}
