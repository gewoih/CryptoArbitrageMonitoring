using Bittrex.Net.Clients;
using Bittrex.Net.SymbolOrderBooks;
using Bybit.Net.SymbolOrderBooks;
using CoreLibrary.Models.Enums;
using CoreLibrary.Models.Exchanges.Base;
using CryptoExchange.Net.Objects;

namespace CoreLibrary.Models.Exchanges
{
	public sealed class BittrexExchange : Exchange
	{
		public override string Name => "Bittrex";
		public override TickersInfo TickersInfo => new("-", CaseType.Uppercase, new("USDT"));
		public override List<CryptoCoin> MarginCoins => new();
		private readonly BittrexClient _client = new();
		private readonly BittrexSocketClient _socketClient = new();

		public override string GetTradeLinkForCoin(CryptoCoin coin, TradeAction tradeAction)
		{
			return $"https://global.bittrex.com/trade/{coin.Name.ToLower()}-usdt";
		}

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
					foreach (var tick in update.Data.Deltas)
					{
						coinPrices[coin].AddTick(tick.Price, tick.Timestamp);
					}
				}));

				var orderBookUpdatingThread = new Thread(async () =>
				{
					var orderBook = new BittrexSymbolOrderBook(ticker, new() { Limit = 25 });
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
				var ticker = GetTickerByCoin(coin);

				if (result.Data.FirstOrDefault(d => d.Symbol == ticker) is null)
					coinPrices.Remove(coin);
			}
		}
	}
}
