using CryptoArbitrageMonitoring.Models;
using Newtonsoft.Json.Linq;

namespace BinanceMonitoring
{
	internal class Program
	{
		private static HttpClient _httpClient = new();
		private const string _binanceApi = "https://api.binance.com/api/v3/ticker/price?symbol=ALGOUSDT";
		private const string _kucoinApi = "https://api.kucoin.com/api/v1/market/orderbook/level1?symbol=ALGO-USDT";
		//private const string _bitfinexApi = "https://api-pub.bitfinex.com/v2/ticker/tALGOUSD";
		private const string _huobiApi = "https://api.huobi.pro/market/trade?symbol=algousdt";
		private const string _gateioApi = "https://api.gateio.ws/api/v4/spot/tickers?currency_pair=ALGO_USDT";
		private const string _okxApi = "https://www.okx.com/api/v5/market/index-tickers?instId=ALGO-USDT";
		private const string _bybitApi = "https://api-testnet.bybit.com/v5/market/mark-price-kline?category=linear&symbol=ALGOUSDT&interval=1&limit=1";
		private const string _bitmartApi = "https://api-cloud.bitmart.com/spot/v1/ticker?symbol=ALGO_USDT";
		private const string _bitstampApi = "https://www.bitstamp.net/api/v2/order_book/algousd/";

		static async Task Main(string[] args)
		{
			var binance = new Exchange("Binance");
			var kucoin = new Exchange("Kucoin");
			//var bitfinex = new Exchange("Bitfinex");
			var huobi = new Exchange("Huobi");
			var gateio = new Exchange("Gateio");
			var okx = new Exchange("Okx");
			var bybit = new Exchange("Bybit");
			var bitmart = new Exchange("Bitmart");
			var bitstamp = new Exchange("Bitstamp");

			var exchangesPrices = new Dictionary<Exchange, decimal>
			{
				{ binance, 0 },
				{ kucoin, 0 },
				//{ bitfinex, 0 },
				{ huobi, 0 },
				{ gateio, 0 },
				{ okx, 0 },
				{ bybit, 0 },
				{ bitmart, 0 },
				{ bitstamp, 0 },
			};

			var exchangesCombinations = GetExchangesCombinations(exchangesPrices.Select(ep => ep.Key).ToList());

			while (true)
			{
				var arbitrageChains = new List<ArbitrageChainInfo>();

				var binanceTask = _httpClient.GetAsync(_binanceApi);
				var kucoinTask = _httpClient.GetAsync(_kucoinApi);
				//var bitfinexTask = _httpClient.GetAsync(_bitfinexApi);
				var huobiTask = _httpClient.GetAsync(_huobiApi);
				var gateioTask = _httpClient.GetAsync(_gateioApi);
				var okxTask = _httpClient.GetAsync(_okxApi);
				var bybitTask = _httpClient.GetAsync(_bybitApi);
				var bitmartTask = _httpClient.GetAsync(_bitmartApi);
				var bitstampTask = _httpClient.GetAsync(_bitstampApi);

				Task.WaitAll(binanceTask, kucoinTask, /*bitfinexTask,*/ huobiTask, gateioTask, okxTask);

				var binanceResult = JObject.Parse(await binanceTask.Result.Content.ReadAsStringAsync());
				var kucoinResult = JObject.Parse(await kucoinTask.Result.Content.ReadAsStringAsync());
				//var bitfinexResult = JArray.Parse(await bitfinexTask.Result.Content.ReadAsStringAsync());
				var huobiResult = JObject.Parse(await huobiTask.Result.Content.ReadAsStringAsync());
				var gateioResult = JArray.Parse(await gateioTask.Result.Content.ReadAsStringAsync());
				var okxResult = JObject.Parse(await okxTask.Result.Content.ReadAsStringAsync());
				var bybitResult = JObject.Parse(await bybitTask.Result.Content.ReadAsStringAsync());
				var bitmartResult = JObject.Parse(await bitmartTask.Result.Content.ReadAsStringAsync());
				var bitstampResult = JObject.Parse(await bitstampTask.Result.Content.ReadAsStringAsync());

				exchangesPrices[binance] = Convert.ToDecimal(binanceResult["price"]);
				exchangesPrices[kucoin] = Convert.ToDecimal(kucoinResult["data"]["price"]);
				//exchangesPrices[bitfinex] = Convert.ToDecimal(bitfinexResult[9]);
				exchangesPrices[huobi] = Convert.ToDecimal(huobiResult["tick"]["data"][0]["price"]);
				exchangesPrices[gateio] = Convert.ToDecimal(gateioResult[0]["last"]);
				exchangesPrices[okx] = Convert.ToDecimal(okxResult["data"][0]["idxPx"]);
				exchangesPrices[bybit] = Convert.ToDecimal(bybitResult["result"]["list"][0][4]);
				exchangesPrices[bitstamp] = (Convert.ToDecimal(bitstampResult["bids"][0][0]) + Convert.ToDecimal(bitstampResult["asks"][0][0])) / 2;

				try
				{
					exchangesPrices[bitmart] = Convert.ToDecimal(bitmartResult["data"]["tickers"][0]["last_price"]);
				}
				catch { }
				
				foreach (var exchangeCombination in exchangesCombinations)
				{
					arbitrageChains.Add(new ArbitrageChainInfo(
						Tuple.Create(exchangeCombination.Item1, exchangesPrices[exchangeCombination.Item1]),
						Tuple.Create(exchangeCombination.Item2, exchangesPrices[exchangeCombination.Item2])));
				}

				var chainWithMaxDivergence = arbitrageChains.MaxBy(c => c.Divergence);

				Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss.fff}]: " +
					$"Max diff. = {chainWithMaxDivergence.Difference}, " +
					$"Max div. = {chainWithMaxDivergence.Divergence}, " +
					$"{chainWithMaxDivergence.MainExchangeInfo.Item1.Name} to {chainWithMaxDivergence.SecondaryExchangeInfo.Item1.Name}");
			}
		}

		private static List<Tuple<Exchange, Exchange>> GetExchangesCombinations(List<Exchange> exchanges)
		{
			var combinations = new List<Tuple<Exchange, Exchange>>();

			for (int i = 0; i < exchanges.Count; i++)
			{
				for (int j = i + 1; j < exchanges.Count; j++)
				{
					combinations.Add(Tuple.Create(exchanges[i], exchanges[j]));
				}
			}

			return combinations;
		}
	}
}

