using CoreLibrary.Models.Enums;
using CoreLibrary.Models.Exchanges.Base;
using CryptoExchange.Net.Objects;
using Kraken.Net.Clients;
using Kraken.Net.SymbolOrderBooks;
using Kucoin.Net.SymbolOrderBooks;

namespace CoreLibrary.Models.Exchanges
{
	public class KrakenExchange : Exchange
	{
		public override string Name => "Kraken";
		public override TickersInfo TickersInfo => new("/", CaseType.Uppercase, new("USD"));
		public override List<CryptoCoin> MarginCoins => new()
		{
			new("BTC"),
			new("ETH"),
			new("MATIC"),
			new("SOL"),
			new("DAI"),
			new("LTC"),
			new("PAXG"),
			new("XRP"),
			new("ADA"),
			new("DOGE"),
			new("DOT"),
			new("XMR"),
			new("FIL"),
			new("AVAX"),
			new("KAVA"),
			new("LINK"),
			new("ATOM"),
			new("BLUR"),
			new("MINA"),
			new("TRX"),
			new("FLOW"),
			new("GRT"),
			new("EOS"),
			new("BCH"),
			new("UNI"),
			new("APT"),
			new("ALGO"),
			new("AAVE"),
			new("SAND"),
			new("XTZ"),
			new("MANA"),
			new("CRV"),
			new("XLM"),
			new("KSM"),
			new("APE"),
			new("ZEC"),
			new("FLR"),
			new("BAT"),
			new("ETC"),
			new("DASH"),
			new("COMP"),
			new("KEEP"),
			new("AXS"),
			new("LRC"),
			new("OMG"),
			new("WAVES"),
			new("SC"),
			new("NANO"),
		};
		private readonly KrakenClient _client = new();
		private readonly KrakenSocketClient _socketClient = new();

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

				var tradesUpdatingThread = new Thread(async () => await _socketClient.SpotStreams.SubscribeToTickerUpdatesAsync(ticker, (update) =>
				{
					coinPrices[coin].AddTick(update.Data.LastTrade.Price, update.Timestamp);
				}));

				var orderBookUpdatingThread = new Thread(async () =>
				{
					var orderBook = new KrakenSpotSymbolOrderBook(ticker, new() { Limit = 25 });
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
			var result = await _client.SpotApi.ExchangeData.GetSymbolsAsync();
			foreach (var coin in coinPrices.Keys.ToList())
			{
				var symbolInfo = result.Data.FirstOrDefault(s => s.Key == GetTickerByCoin(coin).Replace("/", "")).Value;
				if (symbolInfo is null)
					coinPrices.Remove(coin);
			}
		}

		public override string GetTradeLinkForCoin(CryptoCoin coin, TradeAction tradeAction)
		{
			if (tradeAction == TradeAction.Long)
				return $"https://pro.kraken.com/app/trade/{coin.Name.ToLower()}-usd";
			else
				return $"https://pro.kraken.com/app/trade/margin-{coin.Name.ToLower()}-usd";
		}
	}
}
