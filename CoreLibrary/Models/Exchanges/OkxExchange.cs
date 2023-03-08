using CoreLibrary.Models.Enums;
using CoreLibrary.Models.Exchanges.Base;
using Okex.Net;
using Okex.Net.Enums;

namespace CoreLibrary.Models.Exchanges
{
    public sealed class OkxExchange : Exchange
    {
        public override string Name => "Okx";
        public override TickersInfo TickersInfo => new("-", CaseType.Uppercase, new("USDT"));
        private readonly OkexSocketClient _socketClient = new();
        private readonly OkexClient _client = new();

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
                await _socketClient.SubscribeToTradesAsync(ticker, (update) =>
                {
                    coinPrices[coin].AddTick(update.Price, update.Time);

                    //if (coin.Name == "ADA")
                    //    Console.WriteLine(coinPrices[coin].Last.Price);
                });

                await _socketClient.SubscribeToOrderBookAsync(ticker, OkexOrderBookType.OrderBook, (update) =>
                {
                    var isFullOrderBook = update.Action == "snapshot";

                    coinPrices[coin].UpdateOrderBook(
                        update.Bids.Select(b => KeyValuePair.Create(b.Price, b.Quantity)),
                        update.Asks.Select(a => KeyValuePair.Create(a.Price, a.Quantity)),
                        isFullOrderBook);

                    //if (coin.Name == "ADA")
                    //{
                    //    Console.WriteLine(coinPrices[coin].Ask);
                    //    Console.WriteLine();
                    //    Console.WriteLine(coinPrices[coin].Bid);
                    //}
                });
            }
        }

        protected override async Task RemoveCoinsWithoutMarginTrading()
        {
            var result = await _client.GetInstrumentsAsync(OkexInstrumentType.Margin);
            foreach (var coin in coinPrices.Keys.ToList())
            {
                if (result.Data.FirstOrDefault(d => d.Instrument == GetTickerByCoin(coin)) is null)
                    coinPrices.Remove(coin);
            }
        }
    }
}
