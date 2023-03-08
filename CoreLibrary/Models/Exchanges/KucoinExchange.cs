using CoreLibrary.Extensions;
using CoreLibrary.Models.Enums;
using CoreLibrary.Models.Exchanges.Base;
using CryptoExchange.Net.CommonObjects;
using CryptoExchange.Net.Sockets;
using Kucoin.Net.Clients;
using Kucoin.Net.Objects.Models.Spot.Socket;

namespace CoreLibrary.Models.Exchanges
{
	public sealed class KucoinExchange : Exchange
	{
		public override string Name => "Kucoin";
		public override TickersInfo TickersInfo => new("-", CaseType.Uppercase, new("USDT"));
		private readonly KucoinSocketClient _socketClient = new();
		private readonly KucoinClient _client = new();

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
					coinPrices[coin].AddTick((decimal)update.Data.LastPrice, update.Data.Timestamp);
				});

				await _socketClient.SpotStreams.SubscribeToAggregatedOrderBookUpdatesAsync(ticker, (update) =>
				{
					coinPrices[coin].UpdateOrderBook(
						update.Data.Changes.Bids.Select(b => KeyValuePair.Create(b.Price, b.Quantity)),
						update.Data.Changes.Asks.Select(a => KeyValuePair.Create(a.Price, a.Quantity)));
				});
			}
		}

		protected override async Task RemoveCoinsWithoutMarginTrading()
		{
			var result = await _client.SpotApi.ExchangeData.GetSymbolsAsync();
			foreach (var coin in coinPrices.Keys.ToList())
			{
				var symbol = result.Data.FirstOrDefault(d => d.Symbol == GetTickerByCoin(coin));

				if (symbol is null || !symbol.IsMarginEnabled)
					coinPrices.Remove(coin);
			}
		}
	}
}
