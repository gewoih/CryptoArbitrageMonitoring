using CoreLibrary.Models.Enums;
using CoreLibrary.Models.Exchanges.Base;
using Newtonsoft.Json.Linq;

namespace CoreLibrary.Models.Exchanges
{
    public sealed class BinanceExchange : Exchange
    {
        public BinanceExchange(HttpClient httpClient) : base(httpClient)
        {
        }

        public override string Name => "Binance";
        public override ExchangeTickersInfo TickersInfo => new ExchangeTickersInfo("", CaseType.Uppercase, new CryptoCoin("USDT"));
        protected override string BaseApiEndpoint => "https://api.binance.com/api/v3/ticker/bookTicker";
        private string PriceApiEndpoint => "https://api.binance.com/api/v3/ticker/price";

        public override async Task UpdateCoinPrices()
        {
            using var result = await httpClient.GetAsync(BaseApiEndpoint);
            using var result2 = await httpClient.GetAsync(PriceApiEndpoint);
            
            var bidAskArray = JArray.Parse(await result.Content.ReadAsStringAsync());
            var lastPricesArray = JArray.Parse(await result2.Content.ReadAsStringAsync());

            foreach (var coin in coinPrices.Keys.ToList())
            {
                var coinData = bidAskArray.FirstOrDefault(p => p["symbol"].ToString() == GetTickerByCoin(coin));
                var priceData = lastPricesArray.FirstOrDefault(p => p["symbol"].ToString() == GetTickerByCoin(coin));

                if (coinData == null || priceData == null)
                {
                    coinPrices.Remove(coin);
                    continue;
                }

                var bid = Convert.ToDecimal(coinData["bidPrice"]);
                var ask = Convert.ToDecimal(coinData["askPrice"]);
                var last = Convert.ToDecimal(priceData["price"]);

                coinPrices[coin].AddTick(bid, ask, last);
            }
        }
    }
}
