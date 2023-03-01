using CryptoArbitrageMonitoring.Models.Exchanges.Base;
using Newtonsoft.Json.Linq;

namespace CryptoArbitrageMonitoring.Models.Exchanges
{
    public sealed class BinanceExchange : Exchange
    {
        public override string Name => "Binance";
        protected override string _baseApiEndpoint => "https://api.binance.com/api/v3/ticker/bookTicker";
        
        public BinanceExchange(List<CryptoCoin> coins, ExchangeTickersInfo tickersInfo) : base(coins, tickersInfo)
        {
        }

        public override async Task UpdateCoinPrices()
        {
            using var httpClient = new HttpClient();

            var result = await httpClient.GetAsync(_baseApiEndpoint);
            var pricesArray = JArray.Parse(await result.Content.ReadAsStringAsync());

            foreach (var coin in CoinPrices.Keys.ToList())
            {
                var coinData = pricesArray.First(p => p["symbol"].ToString() == GetTickerByCoin(coin));
                var bid = Convert.ToDecimal(coinData["bidPrice"]);
                var ask = Convert.ToDecimal(coinData["askPrice"]);

                CoinPrices[coin] = (bid + ask) / 2;
            }
        }
    }
}
