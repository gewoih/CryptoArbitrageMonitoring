using CryptoArbitrageMonitoring.Models.Exchanges.Base;
using Newtonsoft.Json.Linq;

namespace CryptoArbitrageMonitoring.Models.Exchanges
{
    public sealed class GateioExchange : Exchange
    {
        public override string Name => "Gateio";
        protected override string _baseApiEndpoint => "https://api.gateio.ws/api/v4/spot/tickers";
        
        public GateioExchange(List<CryptoCoin> coins, ExchangeTickersInfo tickersInfo) : base(coins, tickersInfo) { }

        public override async Task UpdateCoinPrices()
        {
            using var httpClient = new HttpClient();

            var result = await httpClient.GetAsync(_baseApiEndpoint);
            var prices = JArray.Parse(await result.Content.ReadAsStringAsync());

            foreach (var coin in CoinPrices.Keys.ToList())
            {
                var coinData = prices.First(p => p["currency_pair"].ToString() == GetTickerByCoin(coin));
                var bid = Convert.ToDecimal(coinData["highest_bid"]);
                var ask = Convert.ToDecimal(coinData["lowest_ask"]);

                CoinPrices[coin] = new MarketData { Bid = bid, Ask = ask };
            }
        }
    }
}
