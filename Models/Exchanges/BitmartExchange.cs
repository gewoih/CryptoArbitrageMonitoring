using CryptoArbitrageMonitoring.Models.Exchanges.Base;
using Newtonsoft.Json.Linq;

namespace CryptoArbitrageMonitoring.Models.Exchanges
{
    public sealed class BitmartExchange : Exchange
    {
        public override string Name => "Bitmart";
        protected override string _baseApiEndpoint => "https://api-cloud.bitmart.com/spot/v1/ticker";
        
        public BitmartExchange(List<CryptoCoin> coins, ExchangeTickersInfo tickersInfo) : base(coins, tickersInfo) { }

        public override async Task UpdateCoinPrices()
        {
            using var httpClient = new HttpClient();

            var result = await httpClient.GetAsync(_baseApiEndpoint);
            var prices = JObject.Parse(await result.Content.ReadAsStringAsync());

            foreach (var coin in CoinPrices.Keys.ToList())
            {
                try
                {
                    var coinData = prices["data"]["tickers"].First(p => p["symbol"].ToString() == GetTickerByCoin(coin));
                    var bid = Convert.ToDecimal(coinData["best_bid"]);
                    var ask = Convert.ToDecimal(coinData["best_ask"]);

                    CoinPrices[coin] = new MarketData { Bid = bid, Ask = ask };
                }
                catch { }
            }
        }
    }
}
