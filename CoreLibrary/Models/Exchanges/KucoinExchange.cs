using CoreLibrary.Models.Enums;
using CoreLibrary.Models.Exchanges.Base;
using Newtonsoft.Json.Linq;

namespace CoreLibrary.Models.Exchanges
{
    public sealed class KucoinExchange : Exchange
    {
        public override string Name => "Kucoin";
        public override TickersInfo TickersInfo => new("-", CaseType.Uppercase, new("USDT"));
        protected override string BaseApiEndpoint => "https://api.kucoin.com/api/v1/market/allTickers";

        public override async Task UpdateCoinPrices()
        {
            if (!IsCoinsWithoutMarginRemoved)
            {
                await RemoveCoinsWithoutMarginTrading();
                IsCoinsWithoutMarginRemoved = true;
            }

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
                var last = Convert.ToDecimal(coinData["last"]);

                coinPrices[coin].AddTick(bid, ask, last);
            }
        }

        protected override async Task RemoveCoinsWithoutMarginTrading()
        {
            using var result = await httpClient.GetAsync("https://api.kucoin.com/api/v1/currencies");
            var symbols = JObject.Parse(await result.Content.ReadAsStringAsync());

            foreach (var coin in coinPrices.Keys.ToList())
            {
                var symbolInfo = symbols["data"].FirstOrDefault(s => s["currency"].ToString().ToUpper() == coin.Name);

                if (symbolInfo == null || !bool.Parse(symbolInfo["isMarginEnabled"].ToString()))
                    coinPrices.Remove(coin);
            }
        }
    }
}
