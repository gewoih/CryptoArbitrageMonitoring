using CoreLibrary.Models.Enums;
using CoreLibrary.Models.Exchanges.Base;
using Kraken.Net.Clients;

namespace CoreLibrary.Models.Exchanges
{
	public class KrakenExchange : Exchange
	{
		public override string Name => "Kraken";
		public override TickersInfo TickersInfo => new("/", CaseType.Uppercase, new("USD"));
		private readonly KrakenClient _client = new();
		private readonly KrakenSocketClient _socketClient = new();

		public override async Task UpdateCoinPrices()
		{
			if (!IsCoinsWithoutMarginRemoved)
			{
				await RemoveCoinsWithoutMarginTrading();
				IsCoinsWithoutMarginRemoved = true;
			}

            var tickers = coinPrices.Keys.Select(GetTickerByCoin);
            await _socketClient.SpotStreams.SubscribeToTickerUpdatesAsync(tickers, (update) =>
			{
				var coin = GetCoinByTicker(update.Topic);
				coinPrices[coin].AddTick(update.Data.LastTrade.Price, update.Timestamp);
            });

			await _socketClient.SpotStreams.SubscribeToOrderBookUpdatesAsync(tickers, 1000, (update) =>
			{
                var coin = GetCoinByTicker(update.Topic);
                coinPrices[coin].UpdateOrderBook(
					update.Data.Bids.Select(b => KeyValuePair.Create(b.Price, b.Quantity)),
					update.Data.Asks.Select(a => KeyValuePair.Create(a.Price, a.Quantity)));
			});
		}

		protected override async Task RemoveCoinsWithoutMarginTrading()
		{
			var result = await _client.SpotApi.ExchangeData.GetSymbolsAsync();
			foreach (var coin in coinPrices.Keys.ToList())
			{
				var symbolInfo = result.Data.FirstOrDefault(s => s.Key == GetTickerByCoin(coin).Replace("/", "")).Value;
				if (symbolInfo is null)
					coinPrices.Remove(coin);
			}
		}
	}
}
