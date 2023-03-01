using CryptoArbitrageMonitoring.Models.Exchanges.Base;
using Newtonsoft.Json.Linq;

namespace CryptoArbitrageMonitoring.Models.Exchanges
{
    public sealed class BybitExchange : Exchange
    {
        public override string Name => "Bybit";
        protected override string _baseApiEndpoint => "https://api-testnet.bybit.com/v5/market/tickers?category=spot";

        public override async Task UpdateCoinPrices()
        {
            using var httpClient = new HttpClient();

            var result = await httpClient.GetAsync(_baseApiEndpoint);
            var prices = JObject.Parse(await result.Content.ReadAsStringAsync());

            foreach (var ticker in CoinPrices.Keys.ToList())
            {
                var coinData = prices["result"]["list"].First(p => p["symbol"].ToString() == CoinsToTickers[ticker]);
                var bid = Convert.ToDecimal(coinData["bid1Price"]);
                var ask = Convert.ToDecimal(coinData["ask1Price"]);

                CoinPrices[ticker] = (bid + ask) / 2;
            }
        }
    }
}
