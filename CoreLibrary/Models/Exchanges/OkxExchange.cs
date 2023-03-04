using CoreLibrary.Models.Enums;
using CoreLibrary.Models.Exchanges.Base;
using Newtonsoft.Json.Linq;

namespace CoreLibrary.Models.Exchanges
{
    public sealed class OkxExchange : Exchange
    {
        public OkxExchange(HttpClient httpClient) : base(httpClient)
        {
        }

        public override string Name => "Okx";
        public override ExchangeTickersInfo TickersInfo => new("-", CaseType.Uppercase, new("USDT"));
        protected override string BaseApiEndpoint => "https://www.okx.com/api/v5/market/tickers?instType=SPOT";

        public override async Task UpdateCoinPrices()
        {
            using var result = await httpClient.GetAsync(BaseApiEndpoint);
            var prices = JObject.Parse(await result.Content.ReadAsStringAsync());

            foreach (var coin in coinPrices.Keys.ToList())
            {
                var coinData = prices["data"].FirstOrDefault(p => p["instId"].ToString() == GetTickerByCoin(coin));

                if (coinData == null)
                {
                    coinPrices.Remove(coin);
                    continue;
                }

                var bid = Convert.ToDecimal(coinData["bidPx"]);
                var ask = Convert.ToDecimal(coinData["askPx"]);
                var last = Convert.ToDecimal(coinData["last"]);

                coinPrices[coin].AddTick(bid, ask, last);
            }
        }
    }
}
