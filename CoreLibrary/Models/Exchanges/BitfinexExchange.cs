using CoreLibrary.Models.Enums;
using CoreLibrary.Models.Exchanges.Base;
using Newtonsoft.Json.Linq;

namespace CoreLibrary.Models.Exchanges
{
    public sealed class BitfinexExchange : Exchange
    {
        public override string Name => "Bitfinex";
        public override TickersInfo TickersInfo => new("", CaseType.Uppercase, new("USD"), "t");
        protected override string BaseApiEndpoint => "https://api-pub.bitfinex.com/v2/tickers?symbols=ALL";

        public override async Task UpdateCoinPrices()
        {
            using var result = await httpClient.GetAsync(BaseApiEndpoint);
            var pricesArray = JArray.Parse(await result.Content.ReadAsStringAsync());

            foreach (var coin in coinPrices.Keys.ToList())
            {
                var coinData = pricesArray.FirstOrDefault(p => p[0].ToString() == GetTickerByCoin(coin));

                if (coinData == null)
                {
                    coinPrices.Remove(coin);
                    continue;
                }

                var bid = Convert.ToDecimal(coinData[1]);
                var ask = Convert.ToDecimal(coinData[3]);
                var last = Convert.ToDecimal(coinData[7]);

                coinPrices[coin].AddTick(bid, ask, last);
            }
        }

        protected override Task RemoveCoinsWithoutMarginTrading()
        {
            throw new NotImplementedException();
        }
    }
}
