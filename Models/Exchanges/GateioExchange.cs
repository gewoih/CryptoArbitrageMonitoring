using CryptoArbitrageMonitoring.Models.Exchanges.Base;
using Newtonsoft.Json.Linq;

namespace CryptoArbitrageMonitoring.Models.Exchanges
{
    public sealed class GateioExchange : Exchange
    {
        public override string Name => "Gateio";
        protected override string _baseApiEndpoint => "https://api.gateio.ws/api/v4/spot/tickers";

        public override async Task UpdateCoinPrices()
        {
            using var httpClient = new HttpClient();

            var result = await httpClient.GetAsync(_baseApiEndpoint);
            var prices = JArray.Parse(await result.Content.ReadAsStringAsync());

            foreach (var ticker in CoinPrices.Keys.ToList())
            {
                var coinData = prices.First(p => p["currency_pair"].ToString() == CoinsToTickers[ticker]);
                var bid = Convert.ToDecimal(coinData["highest_bid"]);
                var ask = Convert.ToDecimal(coinData["lowest_ask"]);

                CoinPrices[ticker] = (bid + ask) / 2;
            }
        }
    }
}
