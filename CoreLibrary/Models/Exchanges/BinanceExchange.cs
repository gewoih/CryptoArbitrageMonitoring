using Binance.Net.Clients;
using CoreLibrary.Models.Enums;
using CoreLibrary.Models.Exchanges.Base;

namespace CoreLibrary.Models.Exchanges
{
	public sealed class BinanceExchange : Exchange
	{
		public override string Name => "Binance";
		public override TickersInfo TickersInfo => new("", CaseType.Uppercase, new CryptoCoin("USDT"));
        private readonly BinanceSocketClient _socketClient = new();
        private readonly BinanceClient _client = new();

		public override async Task UpdateCoinPrices()
		{
            if (!IsCoinsWithoutMarginRemoved)
            {
                await RemoveCoinsWithoutMarginTrading();
                IsCoinsWithoutMarginRemoved = true;
            }

            var tickers = coinPrices.Keys.Select(GetTickerByCoin);

            await _socketClient.SpotStreams.SubscribeToTradeUpdatesAsync(tickers, (update) =>
            {
                var coin = GetCoinByTicker(update.Data.Symbol);
                coinPrices[coin].AddTick(update.Data.Price, update.Data.TradeTime);
            });

            await _socketClient.SpotStreams.SubscribeToOrderBookUpdatesAsync(tickers, 100, (update) =>
            {
                var coin = GetCoinByTicker(update.Data.Symbol);
                coinPrices[coin].UpdateOrderBook(
                        update.Data.Bids.Select(b => KeyValuePair.Create(b.Price, b.Quantity)),
                        update.Data.Asks.Select(a => KeyValuePair.Create(a.Price, a.Quantity)));
            });
        }

		protected override async Task RemoveCoinsWithoutMarginTrading()
		{
            var result = await _client.SpotApi.ExchangeData.GetExchangeInfoAsync();
			foreach (var coin in coinPrices.Keys.ToList())
			{
                var symbol = result.Data.Symbols.FirstOrDefault(s => s.Name == GetTickerByCoin(coin));
                if (symbol == null || !symbol.IsMarginTradingAllowed)
					coinPrices.Remove(coin);
			}
		}
	}
}
