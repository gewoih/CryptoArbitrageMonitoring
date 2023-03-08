using CoreLibrary.Models.Enums;
using CoreLibrary.Models.Exchanges.Base;
using Newtonsoft.Json.Linq;
using System.Collections.Specialized;
using System.Net;
using System.Reactive.Linq;
using System.Text;
using Websocket.Client;

namespace CoreLibrary.Models.Exchanges
{
	public sealed class BitmartExchange : Exchange
	{
		public override string Name => "Bitmart";
		public override TickersInfo TickersInfo => new("_", CaseType.Uppercase, new("USDT"));

		public override async Task UpdateCoinPrices()
		{
			if (!IsCoinsWithoutMarginRemoved)
			{
				await RemoveCoinsWithoutMarginTrading();
				IsCoinsWithoutMarginRemoved = true;
			}

			var url = new Uri("wss://ws-manager-compress.bitmart.com/api?protocol=1.1");
			var clients = new List<WebsocketClient>();

			using var client = new WebsocketClient(url);
			client.ReconnectTimeout = TimeSpan.FromSeconds(15);
			await client.Start();

            client.ReconnectionHappened.Subscribe(message =>
			{
				SubscribeToAllTickers(client);
			});

			client.MessageReceived.Subscribe(message =>
			{
                var result = JObject.Parse(message.Text);
                if (message.Text.Contains("price"))
                {
                    var lastPrice = decimal.Parse(result["data"][0]["last_price"].ToString().Replace(".", ","));
                    var ticker = result["data"][0]["symbol"].ToString();

                    coinPrices[GetCoinByTicker(ticker)].AddTick(lastPrice);

                    Console.WriteLine($"{ticker} new tick {lastPrice}");
                }
                else if (message.Text.Contains("asks"))
                {

                }
				else
				{

					Console.WriteLine(message.Text);
				}
            });

			SubscribeToAllTickers(client);

			await Task.Delay(-1);
        }

		private async void SubscribeToAllTickers(WebsocketClient client)
		{
			var tickersChunks = coinPrices.Keys.Select(c => "\"spot/ticker:" + GetTickerByCoin(c) + "\"").Chunk(20);

			foreach (var chunk in tickersChunks)
			{
				var joinedTickers = string.Join(",", chunk);
				client.Send(@"{""op"": ""subscribe"", ""args"": [" + $"{joinedTickers}" + "]}");
			}
        }

		protected override async Task RemoveCoinsWithoutMarginTrading()
		{
			using var httpClient = new HttpClient();
			using var result = await httpClient.GetAsync("https://api-cloud.bitmart.com/spot/v1/symbols");

			var symbols = JObject.Parse(await result.Content.ReadAsStringAsync());

			foreach (var coin in coinPrices.Keys.ToList())
			{
				var ticker = symbols["data"]["symbols"].FirstOrDefault(s => s.ToString() == GetTickerByCoin(coin));

				if (ticker is null)
					coinPrices.Remove(coin);
			}
		}
	}
}
