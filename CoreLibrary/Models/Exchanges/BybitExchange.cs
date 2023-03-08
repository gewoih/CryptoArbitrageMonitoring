using Bybit.Net.Clients;
using CoreLibrary.Models.Enums;
using CoreLibrary.Models.Exchanges.Base;

namespace CoreLibrary.Models.Exchanges
{
    public sealed class BybitExchange : Exchange
    {
        public override string Name => "Bybit";
        public override TickersInfo TickersInfo => new("", CaseType.Uppercase, new("USDT"));
        private readonly BybitClient _client = new();
        private readonly BybitSocketClient _socketClient = new();

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
                await _socketClient.SpotStreamsV3.SubscribeToTradeUpdatesAsync(ticker, (update) =>
                {
                    coinPrices[coin].AddTick(update.Data.Price, update.Data.Timestamp);
                });

                await _socketClient.SpotStreamsV3.SubscribeToOrderBookUpdatesAsync(ticker, (update) =>
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
            var result = await _client.SpotApiV3.ExchangeData.GetTickersAsync();
            foreach (var coin in coinPrices.Keys.ToList())
            {
                if (result.Data.FirstOrDefault(d => d.Symbol == GetTickerByCoin(coin)) is null)
                    coinPrices.Remove(coin);
            }
        }
    }
}
