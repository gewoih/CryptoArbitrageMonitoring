using CoreLibrary.Models.Enums;
using CoreLibrary.Models.Exchanges.Base;
using Newtonsoft.Json.Linq;

namespace CoreLibrary.Models.Exchanges
{
    public sealed class HuobiExchange : Exchange
    {
        public override string Name => "Huobi";
        public override TickersInfo TickersInfo => new("", CaseType.Lowercase, new("USDT"));
        protected override string BaseApiEndpoint => "https://api.huobi.pro/market/tickers";

        public override async Task UpdateCoinPrices()
        {
            if (!IsCoinsWithoutMarginRemoved)
            {
                await RemoveCoinsWithoutMarginTrading();
                IsCoinsWithoutMarginRemoved = true;
            }
            
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
                var last = Convert.ToDecimal(coinData["close"]);

                coinPrices[coin].AddTick(bid, ask, last);
            }
        }

        protected override async Task RemoveCoinsWithoutMarginTrading()
        {
            using var result = await httpClient.GetAsync("https://api.huobi.pro/v2/settings/common/symbols");
            var symbols = JObject.Parse(await result.Content.ReadAsStringAsync());

            foreach (var coin in coinPrices.Keys.ToList())
            {
                var symbolData = symbols["data"].FirstOrDefault(s => s["sc"].ToString() == GetTickerByCoin(coin));

                if (symbolData == null || symbolData["lr"].ToString() == "null")
                    coinPrices.Remove(coin);
            }
        }
    }
}
