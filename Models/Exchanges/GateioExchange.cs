﻿using CryptoArbitrageMonitoring.Models.Enums;
using CryptoArbitrageMonitoring.Models.Exchanges.Base;
using Newtonsoft.Json.Linq;

namespace CryptoArbitrageMonitoring.Models.Exchanges
{
    public sealed class GateioExchange : Exchange
    {
        public GateioExchange(HttpClient httpClient) : base(httpClient)
        {
        }

        public override string Name => "Gateio";
        public override ExchangeTickersInfo TickersInfo => new("_", CaseType.Uppercase, new("USDT"));
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

                var bid = Convert.ToDecimal(coinData["highest_bid"]);
                var ask = Convert.ToDecimal(coinData["lowest_ask"]);

                coinPrices[coin].Update(bid, ask);
            }
        }
    }
}
