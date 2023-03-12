using Bittrex.Net.Clients;
using CoreLibrary.Models.Enums;
using CoreLibrary.Models.Exchanges.Base;

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

			var tickers = coinPrices.Keys.Select(GetTickerByCoin);

			await _socketClient.SpotStreams.SubscribeToTradeUpdatesAsync(tickers, (update) =>
			{
				var coin = GetCoinByTicker(update.Data.Symbol);

				foreach (var tick in update.Data.Deltas)
				{
					coinPrices[coin].AddTick(tick.Price, tick.Timestamp);
				}
			});

			await _socketClient.SpotStreams.SubscribeToOrderBookUpdatesAsync(tickers, 25, async (update) =>
			{
				var coin = GetCoinByTicker(update.Data.Symbol);

				if (coinPrices[coin].Ask == 0 && coinPrices[coin].Bid == 0)
				{
					var orderBook = await _client.SpotApi.ExchangeData.GetOrderBookAsync(update.Data.Symbol);
					coinPrices[coin].UpdateOrderBook(
						orderBook.Data.Bids.Select(b => KeyValuePair.Create(b.Price, b.Quantity)),
						orderBook.Data.Asks.Select(a => KeyValuePair.Create(a.Price, a.Quantity)),
						true);
				}

				coinPrices[coin].UpdateOrderBook(
					update.Data.BidDeltas.Select(b => KeyValuePair.Create(b.Price, b.Quantity)),
					update.Data.AskDeltas.Select(a => KeyValuePair.Create(a.Price, a.Quantity)));
			});
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
