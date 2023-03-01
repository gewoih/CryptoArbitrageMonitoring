using CryptoArbitrageMonitoring.Models.Exchanges.Base;
using Newtonsoft.Json.Linq;

namespace CryptoArbitrageMonitoring.Models.Exchanges
{
    public sealed class BinanceExchange : Exchange
    {
        public override string Name => "Binance";
        protected override string _baseApiEndpoint => "https://api.binance.com/api/v3/ticker/bookTicker";

        public override async Task UpdateCoinPrices()
        {
            using var httpClient = new HttpClient();

            var result = await httpClient.GetAsync(_baseApiEndpoint);
            var pricesArray = JArray.Parse(await result.Content.ReadAsStringAsync());

            foreach (var ticker in CoinPrices.Keys.ToList())
            {
                var coinData = pricesArray.First(p => p["symbol"].ToString() == CoinsToTickers[ticker]);
                var bid = Convert.ToDecimal(coinData["bidPrice"]);
                var ask = Convert.ToDecimal(coinData["askPrice"]);

                CoinPrices[ticker] = (bid + ask) / 2;
            }
        }
    }
}
