using CryptoArbitrageMonitoring.Models.Enums;
using CryptoArbitrageMonitoring.Models.Exchanges.Base;
using Newtonsoft.Json.Linq;

namespace CryptoArbitrageMonitoring.Models.Exchanges
{
    public sealed class HuobiExchange : Exchange
    {
        public HuobiExchange(HttpClient httpClient) : base(httpClient)
        {
        }

        public override string Name => "Huobi";
        public override ExchangeTickersInfo TickersInfo => new("", CaseType.Lowercase, new("USDT"));
        protected override string BaseApiEndpoint => "https://api.huobi.pro/market/tickers";

        public override async Task UpdateCoinPrices()
        {
            using var result = await httpClient.GetAsync(BaseApiEndpoint);
            var pricesArray = JObject.Parse(await result.Content.ReadAsStringAsync());

            foreach (var coin in coinPrices.Keys.ToList())
            {
                var coinData = pricesArray["data"].FirstOrDefault(p => p["symbol"].ToString() == GetTickerByCoin(coin));

                if (coinData == null)
                {
                    coinPrices.Remove(coin);
                    continue;
                }

                var bid = Convert.ToDecimal(coinData["bid"]);
                var ask = Convert.ToDecimal(coinData["ask"]);

                coinPrices[coin].Update(bid, ask);
            }
        }
    }
}
