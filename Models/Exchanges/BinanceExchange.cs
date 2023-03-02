using CryptoArbitrageMonitoring.Models.Enums;
using CryptoArbitrageMonitoring.Models.Exchanges.Base;
using Newtonsoft.Json.Linq;

namespace CryptoArbitrageMonitoring.Models.Exchanges
{
    public sealed class BinanceExchange : Exchange
    {
        public BinanceExchange(HttpClient httpClient) : base(httpClient)
        {
        }

        public override string Name => "Binance";
        public override ExchangeTickersInfo TickersInfo => new ExchangeTickersInfo("", CaseType.Uppercase, new CryptoCoin("USDT"));
        protected override string BaseApiEndpoint => "https://api.binance.com/api/v3/ticker/bookTicker";

        public override async Task UpdateCoinPrices()
        {
            using var result = await httpClient.GetAsync(BaseApiEndpoint);
            var pricesArray = JArray.Parse(await result.Content.ReadAsStringAsync());

            foreach (var coin in coinPrices.Keys.ToList())
            {
                var coinData = pricesArray.FirstOrDefault(p => p["symbol"].ToString() == GetTickerByCoin(coin));

                if (coinData == null)
                {
                    coinPrices.Remove(coin);
                    continue;
                }

                var bid = Convert.ToDecimal(coinData["bidPrice"]);
                var ask = Convert.ToDecimal(coinData["askPrice"]);

                coinPrices[coin].Update(bid, ask);
            }
        }
    }
}
