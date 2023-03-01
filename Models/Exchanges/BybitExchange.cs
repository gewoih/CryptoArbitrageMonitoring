using CryptoArbitrageMonitoring.Models.Exchanges.Base;
using Newtonsoft.Json.Linq;

namespace CryptoArbitrageMonitoring.Models.Exchanges
{
    public sealed class BybitExchange : Exchange
    {
        public override string Name => "Bybit";
        protected override string _baseApiEndpoint => "https://api-testnet.bybit.com/v5/market/tickers?category=spot";

        public BybitExchange(List<CryptoCoin> coins, ExchangeTickersInfo tickersInfo) : base(coins, tickersInfo) { }
        
        public override async Task UpdateCoinPrices()
        {
            using var httpClient = new HttpClient();

            var result = await httpClient.GetAsync(_baseApiEndpoint);
            var prices = JObject.Parse(await result.Content.ReadAsStringAsync());

            foreach (var coin in CoinPrices.Keys.ToList())
            {
                var coinData = prices["result"]["list"].First(p => p["symbol"].ToString() == GetTickerByCoin(coin));
                var bid = Convert.ToDecimal(coinData["bid1Price"]);
                var ask = Convert.ToDecimal(coinData["ask1Price"]);

                CoinPrices[coin] = new MarketData { Bid = bid, Ask = ask };
            }
        }
    }
}
