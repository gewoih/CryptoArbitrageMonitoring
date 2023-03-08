using CoreLibrary.Models.Enums;
using CoreLibrary.Models.Exchanges.Base;
using Huobi.Net.Clients;

namespace CoreLibrary.Models.Exchanges
{
    public sealed class HuobiExchange : Exchange
    {
        public override string Name => "Huobi";
        public override TickersInfo TickersInfo => new("", CaseType.Lowercase, new("USDT"));
        private readonly HuobiSocketClient _socketClient = new();
        private readonly HuobiClient _client = new();

        public override async Task UpdateCoinPrices()
        {
            if (!IsCoinsWithoutMarginRemoved)
            {
                await RemoveCoinsWithoutMarginTrading();
                IsCoinsWithoutMarginRemoved = true;
            }

            foreach (var coin in coinPrices.Keys)
            {
                var ticker = GetTickerByCoin(coin);
                await _socketClient.SpotStreams.SubscribeToTickerUpdatesAsync(ticker, (update) =>
                {
                    coinPrices[coin].AddTick(update.Data.LastTradePrice, update.Timestamp, (int)update.Data.TradeCount);
                });

                await _socketClient.SpotStreams.SubscribeToOrderBookChangeUpdatesAsync(ticker, 400, (update) =>
                {
                    coinPrices[coin].UpdateOrderBook(
                        update.Data.Bids.Select(b => KeyValuePair.Create(b.Price, b.Quantity)),
                        update.Data.Asks.Select(a => KeyValuePair.Create(a.Price, a.Quantity)));
                });
            }
        }

        protected override async Task RemoveCoinsWithoutMarginTrading()
        {
            var result = await _client.SpotApi.ExchangeData.GetTickersAsync();
            foreach (var coin in coinPrices.Keys.ToList())
            {
                if (result.Data.Ticks.FirstOrDefault(t => t.Symbol == GetTickerByCoin(coin)) is null)
                    coinPrices.Remove(coin);
            }
        }
    }
}
