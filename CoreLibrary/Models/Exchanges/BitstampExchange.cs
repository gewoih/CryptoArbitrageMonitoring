using CoreLibrary.Models;
using CoreLibrary.Models.Enums;
using CoreLibrary.Models.Exchanges.Base;
using Newtonsoft.Json.Linq;

namespace CoreLibrary.Models.Exchanges
{
    public sealed class BitstampExchange : Exchange
    {
        public BitstampExchange(HttpClient httpClient) : base(httpClient)
        {
        }

        public override string Name => "Bitstamp";
        public override ExchangeTickersInfo TickersInfo => new("/", CaseType.Uppercase, new("USD"));
        protected override string BaseApiEndpoint => "https://www.bitstamp.net/api/v2/ticker/";

        public override async Task UpdateCoinPrices()
        {
            using var result = await httpClient.GetAsync(BaseApiEndpoint);
            var pricesArray = JArray.Parse(await result.Content.ReadAsStringAsync());

            foreach (var coin in coinPrices.Keys.ToList())
            {
                var coinData = pricesArray.FirstOrDefault(p => p["pair"].ToString() == GetTickerByCoin(coin));

                if (coinData == null)
                {
                    coinPrices.Remove(coin);
                    continue;
                }

                var bid = Convert.ToDecimal(coinData["bid"]);
                var ask = Convert.ToDecimal(coinData["ask"]);
                var last = Convert.ToDecimal(coinData["last"]);

                coinPrices[coin].AddTick(bid, ask, last);
            }
        }

        protected override Task RemoveCoinsWithoutMarginTrading()
        {
            throw new NotImplementedException();
        }
    }
}
