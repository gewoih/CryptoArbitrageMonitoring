using CoreLibrary.Models.Enums;
using CoreLibrary.Models.Exchanges.Base;
using Okex.Net;
using Okex.Net.Enums;

namespace CoreLibrary.Models.Exchanges
{
    public sealed class OkxExchange : Exchange
    {
        public override string Name => "Okx";
        public override TickersInfo TickersInfo => new("-", CaseType.Uppercase, new("USDT"));
		public override List<CryptoCoin> MarginCoins => new()
        {
            new("1INCH"),
            new("ADA"),
            new("AGLD"),
            new("AKITA"),
            new("ALGO"),
            new("ALPHA"),
            new("ANT"),
            new("APE"),
            new("API3"),
            new("APT"),
            new("AR"),
            new("ATOM"),
            new("AVAX"),
            new("AXS"),
            new("BADGER"),
            new("BAL"),
            new("BAND"),
            new("BAT"),
            new("BCH"),
            new("BETH"),
            new("BLUR"),
            new("BNB"),
            new("BNT"),
            new("BSV"),
            new("BTC"),
            new("CEL"),
            new("CELO"),
            new("CELR"),
            new("CFX"),
            new("CHZ"),
            new("COMP"),
            new("CORE"),
            new("CRO"),
            new("CRV"),
            new("CSPR"),
            new("CVC"),
            new("DASH"),
            new("DOGE"),
            new("DORA"),
            new("DOT"),
            new("DYDX"),
            new("EGLD"),
            new("ELF"),
            new("ENJ"),
            new("ENS"),
            new("EOS"),
            new("ETC"),
            new("ETH"),
            new("ETHW"),
            new("FIL"),
            new("FITFI"),
            new("FLM"),
            new("FLOKI"),
            new("FLOW"),
            new("FTM"),
            new("GALA"),
            new("GFT"),
            new("GLMR"),
            new("GMX"),
            new("GODS"),
            new("GRT"),
            new("HBAR"),
            new("ICP"),
            new("IMX"),
            new("IOST"),
            new("IOTA"),
            new("JST"),
            new("KISHU"),
            new("KLAY"),
            new("KNC"),
            new("KSM"),
            new("LAT"),
            new("LDO"),
            new("LINK"),
            new("LOOKS"),
            new("LPT"),
            new("LRC"),
            new("LTC"),
            new("LUNA"),
            new("LUNC"),
            new("MAGIC"),
            new("MANA"),
            new("MASK"),
            new("MATIC"),
            new("MINA"),
            new("MKR"),
            new("NEAR"),
            new("NEO"),
            new("NFT"),
            new("OKB"),
            new("OMG"),
            new("ONT"),
            new("OP"),
            new("PEOPLE"),
            new("PERP"),
            new("QTUM"),
            new("REN"),
            new("RSR"),
            new("RSS3"),
            new("RVN"),
            new("SAND"),
            new("SHIB"),
            new("SKL"),
            new("SLP"),
            new("SNT"),
            new("SNX"),
            new("SOL"),
            new("STARL"),
            new("STORJ"),
            new("STX"),
            new("SUSHI"),
            new("SWEAT"),
            new("THETA"),
            new("TON"),
            new("TRB"),
            new("TRX"),
            new("UMA"),
            new("UNI"),
            new("USDC"),
            new("USTC"),
            new("WAVES"),
            new("WOO"),
            new("XCH"),
            new("XLM"),
            new("XMR"),
            new("XRP"),
            new("XTZ"),
            new("YFI"),
            new("YFII"),
            new("YGG"),
            new("ZEC"),
            new("ZEN"),
            new("ZIL"),
            new("ZRX"),
		};
		private readonly OkexSocketClient _socketClient = new();
        private readonly OkexClient _client = new();

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
                await _socketClient.SubscribeToTradesAsync(ticker, (update) =>
                {
                    coinPrices[coin].AddTick(update.Price, update.Time);
                });

                await _socketClient.SubscribeToOrderBookAsync(ticker, OkexOrderBookType.OrderBook, (update) =>
                {
                    var isFullOrderBook = update.Action == "snapshot";

                    coinPrices[coin].UpdateOrderBook(
                        update.Bids.Select(b => KeyValuePair.Create(b.Price, b.Quantity)),
                        update.Asks.Select(a => KeyValuePair.Create(a.Price, a.Quantity)),
                        isFullOrderBook);
                });
            }
        }

        protected override async Task RemoveNonExistentCoins()
        {
            var result = await _client.GetInstrumentsAsync(OkexInstrumentType.Spot);
            foreach (var coin in coinPrices.Keys.ToList())
            {
                if (result.Data.FirstOrDefault(d => d.Instrument == GetTickerByCoin(coin)) is null)
                    coinPrices.Remove(coin);
            }
        }

		public override string GetTradeLinkForCoin(CryptoCoin coin, TradeAction tradeAction)
		{
            if (tradeAction == TradeAction.Long)
                return $"https://www.okx.com/ru/trade-spot/{coin.Name.ToLower()}-usdt";
            else
                return $"https://www.okx.com/ru/trade-margin/{coin.Name.ToLower()}-usdt";
		}
	}
}
