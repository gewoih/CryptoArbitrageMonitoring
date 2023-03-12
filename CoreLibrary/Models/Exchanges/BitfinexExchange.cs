using Bitfinex.Net.Clients;
using Bitfinex.Net.Enums;
using CoreLibrary.Models.Enums;
using CoreLibrary.Models.Exchanges.Base;

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
                await _socketClient.SpotStreams.SubscribeToTradeUpdatesAsync(ticker, (update) =>
                {
                    foreach (var trade in update.Data)
                    {
                        if (trade.UpdateType == BitfinexEventType.TradeExecuted)
                            coinPrices[coin].AddTick(trade.Price, trade.Timestamp);
                    }
                });

                await _socketClient.SpotStreams.SubscribeToOrderBookUpdatesAsync(ticker, Precision.PrecisionLevel0, Frequency.Realtime, 100, (update) =>
                {
                    coinPrices[coin].UpdateOrderBook(
                        update.Data.Where(d => d.Quantity >= 0).Select(b => KeyValuePair.Create(b.Price, b.Count != 0 ? Math.Abs(b.Quantity) : 0)),
                        update.Data.Where(d => d.Quantity < 0).Select(a => KeyValuePair.Create(a.Price, a.Count != 0 ? Math.Abs(a.Quantity) : 0)));
                });
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
