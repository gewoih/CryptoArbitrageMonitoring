using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace BinanceMonitoring
{
    internal class Program
    {
        private static HttpClient _httpClient = new();
        private const string _binanceApiEndpoint = "https://api.binance.com/api/v3/ticker/price?symbol=DOTUSDT";
        private const string _kucoinApiEndpoint = "https://api.kucoin.com/api/v1/market/orderbook/level1?symbol=DOT-USDT";
        private const string _bitfinexApiEndpoint = "https://api-pub.bitfinex.com/v2/ticker/tDOTUSD";

        static async Task Main(string[] args)
        {
            var stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < 100; i++)
            {
                var tasks = new Task<HttpResponseMessage>[]
                {
                    _httpClient.GetAsync(_kucoinApiEndpoint),
                    _httpClient.GetAsync(_bitfinexApiEndpoint),
                };

                Task.WaitAll(tasks);

                var symbolInfo = JObject.Parse(await tasks[0].Result.Content.ReadAsStringAsync());
                var kucoinPrice = Convert.ToDecimal(symbolInfo["data"]["price"]);

                symbolInfo = JObject.Parse(await tasks[1].Result.Content.ReadAsStringAsync());
                var bitfinexPrice = Convert.ToDecimal(symbolInfo[9]);

                Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss.fff}]: " +
                    $"{kucoinPrice}; " +
                    $"{bitfinexPrice}; " +
                    $"{kucoinPrice - bitfinexPrice}; " +
                    $"{Math.Round(kucoinPrice / bitfinexPrice * 100 - 100, 2)}%");
            }

            Console.WriteLine(stopwatch.ElapsedMilliseconds / 100);
        }
    }
}

