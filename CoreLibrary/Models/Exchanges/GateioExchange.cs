using CoreLibrary.Models.Enums;
using CoreLibrary.Models.Exchanges.Base;
using Gate.IO.Api;
using Gate.IO.Api.Enums;
using Newtonsoft.Json.Linq;
using Websocket.Client;

namespace CoreLibrary.Models.Exchanges
{
    public sealed class GateioExchange : Exchange
    {
        public override string Name => "Gateio";
        public override TickersInfo TickersInfo => new("_", CaseType.Uppercase, new("USDT"));
        private readonly GateRestApiClient _client = new();
        private readonly GateStreamClient _socketClient = new();

        public override async Task UpdateCoinPrices()
        {
            if (!IsCoinsWithoutMarginRemoved)
            {
                await RemoveCoinsWithoutMarginTrading();
                IsCoinsWithoutMarginRemoved = true;
            }

            var socketClient = new WebsocketClient(new Uri("wss://api.gateio.ws/ws/v4/"));
            await socketClient.Start();

            socketClient.MessageReceived.Subscribe((message) =>
            {
                if (!message.Text.Contains("price"))
                    return;

                var newTick = JObject.Parse(message.Text);

                var coin = GetCoinByTicker(newTick["result"]["currency_pair"].ToString());
                coinPrices[coin].AddTick(decimal.Parse(newTick["result"]["price"].ToString()), DateTime.UtcNow);
            });

            var tickers = string.Join(",", coinPrices.Keys.Select(c => "\"" + GetTickerByCoin(c) + "\""));
            socketClient.Send(@"{ ""channel"": ""spot.trades"", ""event"": ""subscribe"", ""payload"": [" + tickers + "]}");
            
            foreach (var coin in coinPrices.Keys)
            {
                var ticker = GetTickerByCoin(coin);
                await _socketClient.Spot.SubscribeToOrderBookSnapshotsAsync(ticker, 100, 100, (update) =>
                {
                    coinPrices[coin].UpdateOrderBook(
                        update.Data.Bids.Select(b => KeyValuePair.Create(b.Price, b.Quantity)),
                        update.Data.Asks.Select(a => KeyValuePair.Create(a.Price, a.Quantity)),
                        true);
                });
            }
        }

        protected override async Task RemoveCoinsWithoutMarginTrading()
        {
            var result = await _client.Spot.GetAllPairsAsync();
            foreach (var coin in coinPrices.Keys.ToList())
            {
                var pair = result.Data.FirstOrDefault(d => d.Symbol == GetTickerByCoin(coin));
                if (pair is null || pair.Status == SpotMarketStatus.Untradable)
                    coinPrices.Remove(coin);
            }
        }
    }
}
