using Bybit.Net.Clients;
using CoreLibrary.Models.Enums;
using CoreLibrary.Models.Exchanges.Base;

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
                await _socketClient.SpotStreamsV3.SubscribeToTradeUpdatesAsync(ticker, (update) =>
                {
                    coinPrices[coin].AddTick(update.Data.Price, update.Data.Timestamp);
                });

                await _socketClient.SpotStreamsV3.SubscribeToOrderBookUpdatesAsync(ticker, (update) =>
                {
                    coinPrices[coin].UpdateOrderBook(
                        update.Data.Bids.Select(b => KeyValuePair.Create(b.Price, b.Quantity)),
                        update.Data.Asks.Select(a => KeyValuePair.Create(a.Price, a.Quantity)),
                        true);
                });
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
