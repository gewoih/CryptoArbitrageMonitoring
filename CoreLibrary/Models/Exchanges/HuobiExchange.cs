using CoreLibrary.Models.Enums;
using CoreLibrary.Models.Exchanges.Base;
using Huobi.Net.Clients;

namespace CoreLibrary.Models.Exchanges
{
    public sealed class HuobiExchange : Exchange
    {
        public override string Name => "Huobi";
        public override TickersInfo TickersInfo => new("", CaseType.Lowercase, new("USDT"));
		public override List<CryptoCoin> MarginCoins => new()
        {
			new("SHIB"),
            new("MATIC"),
            new("FIL"),
            new("DOGE"),
            new("SOL"),
            new("DYDX"),
            new("ATOM"),
            new("NEAR"),
            new("AVAX"),
            new("XLM"),
            new("BTT"),
            new("ALGO"),
            new("SAND"),
            new("MANA"),
            new("VET"),
            new("FLOW"),
            new("UNI"),
            new("THETA"),
            new("CRV"),
            new("FTT"),
            new("SUSHI"),
            new("ZIL"),
            new("NEO"),
            new("KSM"),
            new("YFI"),
            new("WAVES"),
            new("AAVE"),
            new("IOST"),
            new("QTUM"),
            new("AXS"),
            new("XTZ"),
            new("OMG"),
            new("COMP"),
            new("YFII"),
            new("RSR"),
            new("ONT"),
            new("WICC"),
            new("BAND"),
            new("BNB"),
            new("FTM"),
            new("LOOKS"),
            new("PEOPLE"),
            new("BTC"),
            new("ETH"),
            new("HT"),
            new("DOT"),
            new("XRP"),
            new("LINK"),
            new("BCH"),
            new("LTC"),
            new("BSV"),
            new("ADA"),
            new("EOS"),
            new("TRX"),
            new("ETC"),
            new("CORE"),
            new("BLUR"),
            new("TON"),
            new("BONK"),
            new("APT"),
            new("OP"),
            new("ETHW"),
            new("LDO"),
            new("APE"),
            new("MINA"),
            new("NYM"),
            new("OAS"),
            new("PHB"),
            new("KRIPTO"),
            new("AMP"),
            new("XCH"),
            new("ACH"),
            new("ICP"),
            new("GRT"),
            new("NFT"),
            new("HBAR"),
            new("ANKR"),
            new("1INCH"),
            new("CHZ"),
            new("SUN"),
            new("AR"),
            new("SNX"),
            new("SXP"),
            new("ONE"),
            new("KAVA"),
            new("BAT"),
            new("JST"),
            new("ZRX"),
            new("INJ"),
            new("ANT"),
            new("ELF"),
            new("NEST"),
            new("STORJ"),
            new("OGN"),
            new("REN"),
            new("RNDR"),
            new("REEF"),
            new("IMX"),
            new("WIN"),
            new("JASMY"),
            new("EGLD"),
            new("GLMR"),
            new("CELO"),
            new("C98"),
            new("GARI"),
            new("WALLET"),
		};
		private readonly HuobiSocketClient _socketClient = new();
        private readonly HuobiClient _client = new();

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
                await _socketClient.SpotStreams.SubscribeToTickerUpdatesAsync(ticker, (update) =>
                {
                    coinPrices[coin].AddTick(update.Data.LastTradePrice, update.Timestamp, (int)update.Data.TradeCount);
                });

                await _socketClient.SpotStreams.SubscribeToOrderBookChangeUpdatesAsync(ticker, 400, (update) =>
                {
                    coinPrices[coin].UpdateOrderBook(
                        update.Data.Bids.Select(b => KeyValuePair.Create(b.Price, b.Quantity)),
                        update.Data.Asks.Select(a => KeyValuePair.Create(a.Price, a.Quantity)));
                });
            }
        }

        protected override async Task RemoveNonExistentCoins()
        {
            var result = await _client.SpotApi.ExchangeData.GetTickersAsync();
            foreach (var coin in coinPrices.Keys.ToList())
            {
                if (result.Data.Ticks.FirstOrDefault(t => t.Symbol == GetTickerByCoin(coin)) is null)
                    coinPrices.Remove(coin);
            }
        }

		public override string GetTradeLinkForCoin(CryptoCoin coin, TradeAction tradeAction)
		{
            if (tradeAction == TradeAction.Long)
                return $"https://www.huobi.com/en-us/exchange/{coin.Name.ToLower()}_usdt";
            else
                return $"https://www.huobi.com/en-us/margin/{coin.Name.ToLower()}_usdt";
		}
	}
}
