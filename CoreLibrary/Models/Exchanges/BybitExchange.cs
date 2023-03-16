using Bybit.Net.Clients;
using Bybit.Net.SymbolOrderBooks;
using CoreLibrary.Models.Enums;
using CoreLibrary.Models.Exchanges.Base;
using CryptoExchange.Net.Objects;
using Huobi.Net.SymbolOrderBooks;

namespace CoreLibrary.Models.Exchanges
{
    public sealed class BybitExchange : Exchange
    {
        public override string Name => "Bybit";
        public override TickersInfo TickersInfo => new("", CaseType.Uppercase, new("USDT"));
		public override List<CryptoCoin> MarginCoins => new()
        {
		    new("BTC"),
            new("ETH"),
            new("BIT"),
            new("XRP"),
            new("OP"),
            new("CHZ"),
            new("AAVE"),
            new("ADA"),
            new("ALGO"),
            new("APE"),
            new("APT"),
            new("AR"),
            new("ATOM"),
            new("AVAX"),
            new("AXS"),
            new("BAT"),
            new("BCH"),
            new("BICO"),
            new("BLUR"),
            new("BNB"),
            new("BUSD"),
            new("COMP"),
            new("CORE"),
            new("CRV"),
            new("DAI"),
            new("DOGE"),
            new("DOT"),
            new("DYDX"),
            new("EGLD"),
            new("ENS"),
            new("EOS"),
            new("ETC"),
            new("FIL"),
            new("FLOW"),
            new("FTM"),
            new("GALA"),
            new("GMT"),
            new("GRT"),
            new("ICP"),
            new("IMX"),
            new("JASMY"),
            new("LINK"),
            new("LTC"),
            new("LUNC"),
            new("MANA"),
            new("MATIC"),
            new("NEAR"),
            new("QNT"),
            new("SAND"),
            new("SHIB"),
            new("SLP"),
            new("SOL"),
            new("SUSHI"),
            new("THETA"),
            new("TRX"),
            new("UNI"),
            new("USDC"),
            new("USDD"),
            new("WAVES"),
            new("XLM"),
            new("YFI"),
            new("ZIL"),
            new("ZRX"),
		};
		private readonly BybitClient _client = new();
        private readonly BybitSocketClient _socketClient = new();

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
				var tradesUpdatingThread = new Thread(async () => await _socketClient.SpotStreamsV3.SubscribeToTradeUpdatesAsync(ticker, (update) =>
                {
                    coinPrices[coin].AddTick(update.Data.Price, update.Data.Timestamp);
                }));

				var orderBookUpdatingThread = new Thread(async () =>
				{
					var orderBook = new BybitSpotSymbolOrderBook(ticker, new() { Limit = 25 });
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
            var result = await _client.SpotApiV3.ExchangeData.GetTickersAsync();
            foreach (var coin in coinPrices.Keys.ToList())
            {
                if (result.Data.FirstOrDefault(d => d.Symbol == GetTickerByCoin(coin)) is null)
                    coinPrices.Remove(coin);
            }
        }

		public override string GetTradeLinkForCoin(CryptoCoin coin, TradeAction tradeAction)
		{
			return $"https://www.bybit.com/en-US/trade/spot/{coin.Name}/USDT";
		}
	}
}
