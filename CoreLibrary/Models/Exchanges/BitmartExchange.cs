﻿using CoreLibrary.Models.Enums;
using CoreLibrary.Models.Exchanges.Base;
using Newtonsoft.Json.Linq;

namespace CoreLibrary.Models.Exchanges
{
    public sealed class BitmartExchange : Exchange
    {
        public override string Name => "Bitmart";
        public override TickersInfo TickersInfo => new("_", CaseType.Uppercase, new("USDT"));
        protected override string BaseApiEndpoint => "https://api-cloud.bitmart.com/spot/v2/ticker";

        public override async Task UpdateCoinPrices()
        {
            using var result = await httpClient.GetAsync(BaseApiEndpoint);
            var prices = JObject.Parse(await result.Content.ReadAsStringAsync());

            foreach (var coin in coinPrices.Keys.ToList())
            {
                var coinData = prices["data"]["tickers"].FirstOrDefault(p => p["symbol"].ToString() == GetTickerByCoin(coin));

                if (coinData == null)
                {
                    coinPrices.Remove(coin);
                    continue;
                }

                var bid = Convert.ToDecimal(coinData["best_bid"]);
                var ask = Convert.ToDecimal(coinData["best_ask"]);
                var last = Convert.ToDecimal(coinData["last_price"]);

                coinPrices[coin].AddTick(bid, ask, last);
            }
        }

        protected override Task RemoveCoinsWithoutMarginTrading()
        {
            throw new NotImplementedException();
        }
    }
}
