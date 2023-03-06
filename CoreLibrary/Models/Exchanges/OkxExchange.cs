﻿using CoreLibrary.Models.Enums;
using CoreLibrary.Models.Exchanges.Base;
using Newtonsoft.Json.Linq;

namespace CoreLibrary.Models.Exchanges
{
    public sealed class OkxExchange : Exchange
    {
        public override string Name => "Okx";
        public override TickersInfo TickersInfo => new("-", CaseType.Uppercase, new("USDT"));
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

            if (!IsCoinsWithoutMarginRemoved)
            {
                await RemoveCoinsWithoutMarginTrading();
                IsCoinsWithoutMarginRemoved = true;
            }
        }

        protected override async Task RemoveCoinsWithoutMarginTrading()
        {
            using var result = await httpClient.GetAsync("https://www.okx.com/api/v5/public/instruments?instType=MARGIN");
            var coinsData = JObject.Parse(await result.Content.ReadAsStringAsync());

            foreach (var coin in coinPrices.Keys.ToList())
            {
                var coinData = coinsData["data"].FirstOrDefault(d => d["instId"].ToString() == GetTickerByCoin(coin));
                
                if (coinData == null)
                    coinPrices.Remove(coin);
            }
        }
    }
}
