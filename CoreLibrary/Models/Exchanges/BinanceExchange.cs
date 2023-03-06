using CoreLibrary.Models.Enums;
using CoreLibrary.Models.Exchanges.Base;
using Newtonsoft.Json.Linq;

namespace CoreLibrary.Models.Exchanges
{
	public sealed class BinanceExchange : Exchange
	{
		public override string Name => "Binance";
		public override TickersInfo TickersInfo => new("", CaseType.Uppercase, new CryptoCoin("USDT"));
		protected override string BaseApiEndpoint => "https://api.binance.com/api/v3/ticker/bookTicker";

		public override async Task UpdateCoinPrices()
		{
            if (!IsCoinsWithoutMarginRemoved)
            {
            	await RemoveCoinsWithoutMarginTrading();
            	IsCoinsWithoutMarginRemoved = true;
            }

            using var result = await httpClient.GetAsync(BaseApiEndpoint);
            using var result2 = await httpClient.GetAsync("https://api.binance.com/api/v3/ticker/price");

            var bidAskArray = JArray.Parse(await result.Content.ReadAsStringAsync());
            var lastPricesArray = JArray.Parse(await result2.Content.ReadAsStringAsync());

            foreach (var coin in coinPrices.Keys.ToList())
            {
            	var coinData = bidAskArray.FirstOrDefault(p => p["symbol"].ToString() == GetTickerByCoin(coin));
            	var priceData = lastPricesArray.FirstOrDefault(p => p["symbol"].ToString() == GetTickerByCoin(coin));

            	if (coinData == null || priceData == null)
            	{
            		coinPrices.Remove(coin);
            		continue;
            	}

            	var bid = Convert.ToDecimal(coinData["bidPrice"]);
            	var ask = Convert.ToDecimal(coinData["askPrice"]);
            	var last = Convert.ToDecimal(priceData["price"]);

            	coinPrices[coin].AddTick(bid, ask, last);
            }
        }

		protected override async Task RemoveCoinsWithoutMarginTrading()
		{
			using var result = await httpClient.GetAsync("https://api.binance.com/api/v3/exchangeInfo");
			var coinsData = JObject.Parse(await result.Content.ReadAsStringAsync());

			foreach (var coin in coinPrices.Keys.ToList())
			{
				var coinData = coinsData["symbols"].FirstOrDefault(d => d["symbol"].ToString() == GetTickerByCoin(coin));
				if (coinData == null || !bool.Parse(coinData["isMarginTradingAllowed"].ToString()))
				{
					coinPrices.Remove(coin);
					continue;
				}
			}
		}
	}
}
