using CoreLibrary.Models.Enums;
using CoreLibrary.Models.Exchanges.Base;
using Newtonsoft.Json.Linq;

namespace CoreLibrary.Models.Exchanges
{
    public sealed class GateioExchange : Exchange
    {
        public override string Name => "Gateio";
        public override TickersInfo TickersInfo => new("_", CaseType.Uppercase, new("USDT"));
        protected override string BaseApiEndpoint => "https://api.gateio.ws/api/v4/spot/tickers";

        public override async Task UpdateCoinPrices()
        {
            using var result = await httpClient.GetAsync(BaseApiEndpoint);
            var prices = JArray.Parse(await result.Content.ReadAsStringAsync());

            foreach (var coin in coinPrices.Keys.ToList())
            {
                var coinData = prices.FirstOrDefault(p => p["currency_pair"].ToString() == GetTickerByCoin(coin));

                if (coinData == null)
                {
                    coinPrices.Remove(coin);
                    continue;
                }

                var bid = 0m;
                var ask = 0m;
                var last = 0m;
                try
                {
                    bid = Convert.ToDecimal(coinData["highest_bid"]);
                    ask = Convert.ToDecimal(coinData["lowest_ask"]);
                    last = Convert.ToDecimal(coinData["last"]);
                }
                catch
                {
                    coinPrices.Remove(coin);
                    continue;
                }
             
                coinPrices[coin].AddTick(bid, ask, last);
            }
        }

        protected override Task RemoveCoinsWithoutMarginTrading()
        {
            throw new NotImplementedException();
        }
    }
}
