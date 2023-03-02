using CoreLibrary.Models;
using CoreLibrary.Models.Enums;
using CoreLibrary.Models.Exchanges.Base;
using Newtonsoft.Json.Linq;

namespace CoreLibrary.Models.Exchanges
{
    public sealed class KucoinExchange : Exchange
    {
        public KucoinExchange(HttpClient httpClient) : base(httpClient)
        {
        }

        public override string Name => "Kucoin";
        public override ExchangeTickersInfo TickersInfo => new("-", CaseType.Uppercase, new("USDT"));
        protected override string BaseApiEndpoint => "https://api.kucoin.com/api/v1/market/allTickers";

        public override async Task UpdateCoinPrices()
        {
            using var result = await httpClient.GetAsync(BaseApiEndpoint);
            var prices = JObject.Parse(await result.Content.ReadAsStringAsync());

            foreach (var coin in coinPrices.Keys.ToList())
            {
                var coinData = prices["data"]["ticker"].FirstOrDefault(p => p["symbol"].ToString() == GetTickerByCoin(coin));

                if (coinData == null)
                {
                    coinPrices.Remove(coin);
                    continue;
                }

                var bid = Convert.ToDecimal(coinData["buy"]);
                var ask = Convert.ToDecimal(coinData["sell"]);

                coinPrices[coin].Update(bid, ask);
            }
        }
    }
}
