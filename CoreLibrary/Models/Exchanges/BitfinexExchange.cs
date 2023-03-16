using Binance.Net.Objects;
using Binance.Net.SymbolOrderBooks;
using Bitfinex.Net.Clients;
using Bitfinex.Net.Enums;
using Bitfinex.Net.Objects;
using Bitfinex.Net.SymbolOrderBooks;
using CoreLibrary.Models.Enums;
using CoreLibrary.Models.Exchanges.Base;
using CryptoExchange.Net.Objects;

namespace CoreLibrary.Models.Exchanges
{
    public sealed class BitfinexExchange : Exchange
    {
        public override string Name => "Bitfinex";
        public override TickersInfo TickersInfo => new("", CaseType.Uppercase, new("USD"), "t");
		public override List<CryptoCoin> MarginCoins => new()
        {
		    new("BTC"),
            new("LTC"),
            new("ETH"),
            new("ETC"),
            new("ZEC"),
            new("XMR"),
            new("DASH"),
            new("XRP"),
            new("IOTA"),
            new("EOS"),
            new("OMG"),
            new("NEO"),
            new("PNT"),
            new("ZRX"),
            new("TRX"),
            new("DAI"),
            new("XLM"),
            new("MKR"),
            new("XTZ"),
            new("USDt"),
            new("ATOM"),
            new("LEO"),
            new("ALGO"),
            new("XAUt"),
            new("DOT"),
            new("ADA"),
            new("LINK"),
            new("COMP"),
            new("UNI"),
            new("AVAX"),
            new("EGLD"),
            new("YFI"),
            new("FIL"),
            new("BCHN"),
            new("SUSHI"),
            new("SOL"),
            new("DOGE"),
            new("FTM"),
            new("MATIC"),
            new("AXS"),
            new("SHIB"),
            new("APE"),
            new("ETHW"),
            new("APT"),
		};
		private readonly BitfinexClient _client = new();
        private readonly BitfinexSocketClient _socketClient = new();

        public override async Task StartUpdatingMarketData()
        {
            if (!IsNonExistentCoinsRemoved)
            {
                await RemoveNonExistentCoins();
                IsNonExistentCoinsRemoved = true;
            }

            foreach (var coin in coinPrices.Keys)
            {
                var ticker = GetTickerByCoin(coin);
				var tradesUpdatingThread = new Thread(async () => await _socketClient.SpotStreams.SubscribeToTradeUpdatesAsync(ticker, (update) =>
                {
                    foreach (var trade in update.Data)
                    {
                        if (trade.UpdateType == BitfinexEventType.TradeExecuted)
                            coinPrices[coin].AddTick(trade.Price, trade.Timestamp);
                    }
                }));

				var orderBookUpdatingThread = new Thread(async () =>
				{
					var orderBook = new BitfinexSymbolOrderBook(ticker, new BitfinexOrderBookOptions() { Limit = 25 });
					orderBook.OnStatusChange += (oldState, newState) =>
					{
						if (newState != OrderBookStatus.Synced)
							coinPrices[coin].ClearOrderBook();
					};
					var startResult = await orderBook.StartAsync();

					if (!startResult.Success)
						Console.WriteLine($"Failed to start updating order book for exchange '{Name}'!");

					orderBook.OnOrderBookUpdate += (update) =>
					{
						coinPrices[coin].UpdateOrderBook(
							update.Bids.Select(b => KeyValuePair.Create(b.Price, b.Quantity)),
							update.Asks.Select(a => KeyValuePair.Create(a.Price, a.Quantity)),
							true);
					};
				});

                tradesUpdatingThread.Start();
                orderBookUpdatingThread.Start();
			}
        }

        protected override async Task RemoveNonExistentCoins()
        {
            var result = await _client.SpotApi.ExchangeData.GetTickersAsync();
            foreach (var coin in coinPrices.Keys.ToList())
            {
                if (result.Data.FirstOrDefault(d => d.Symbol == GetTickerByCoin(coin)) is null)
                    coinPrices.Remove(coin);
            }
        }

		public override string GetTradeLinkForCoin(CryptoCoin coin, TradeAction tradeAction)
		{
			if (tradeAction == TradeAction.Long)
				return $"https://trading.bitfinex.com/t/{coin.Name}:USD?type=exchange";
			else
				return $"https://trading.bitfinex.com/t/{coin.Name}:USD?type=margin";
		}
	}
}
