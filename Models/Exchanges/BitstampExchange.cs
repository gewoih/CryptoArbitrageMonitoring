using CryptoArbitrageMonitoring.Models.Exchanges.Base;
using Newtonsoft.Json.Linq;

namespace CryptoArbitrageMonitoring.Models.Exchanges
{
	public sealed class BitstampExchange : Exchange
	{
		public override string Name => "Bitstamp";
		protected override string _baseApiEndpoint => "https://www.bitstamp.net/api/v2/ticker/";
		
		public BitstampExchange(List<CryptoCoin> coins, ExchangeTickersInfo tickersInfo) : base(coins, tickersInfo) { }

		public override async Task UpdateCoinPrices()
		{
			using var httpClient = new HttpClient();

			var result = await httpClient.GetAsync(_baseApiEndpoint);
			var pricesArray = JArray.Parse(await result.Content.ReadAsStringAsync());

			foreach (var coin in CoinPrices.Keys.ToList())
			{
				var coinData = pricesArray.First(p => p["pair"].ToString() == GetTickerByCoin(coin));
				var bid = Convert.ToDecimal(coinData["bid"]);
				var ask = Convert.ToDecimal(coinData["ask"]);

				CoinPrices[coin] = (bid + ask) / 2;
			}
		}
	}
}
