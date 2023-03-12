using Binance.Net.Clients;
using CoreLibrary.Models.Enums;
using CoreLibrary.Models.Exchanges.Base;

namespace CoreLibrary.Models.Exchanges
{
	public sealed class BinanceExchange : Exchange
	{
		public override string Name => "Binance";
		public override TickersInfo TickersInfo => new("", CaseType.Uppercase, new CryptoCoin("USDT"));
        public override List<CryptoCoin> MarginCoins => new()
        {
            new("1INCH"),
            new("ACH"),
            new("ADA"),
            new("AGIX"),
            new("AKRO"),
            new("ALCX"),
            new("ALGO"),
            new("ALICE"),
            new("ALPHA"),
            new("AMB"),
            new("ANKR"),
            new("ANT"),
            new("APE"),
            new("API3"),
            new("APT"),
            new("AR"),
            new("ARPA"),
            new("ASTR"),
            new("ATOM"),
            new("AUCTION"),
            new("AUDIO"),
            new("AVAX"),
            new("AXS"),
            new("BADGER"),
            new("BAKE"),
            new("BAL"),
            new("BAND"),
            new("BAT"),
            new("BCH"),
            new("BEL"),
            new("BNB"),
            new("BNT"),
            new("BOND"),
            new("BSW"),
            new("BTC"),
            new("BTS"),
            new("BURGER"),
            new("BUSD"),
            new("C98"),
            new("CAKE"),
            new("CELO"),
            new("CELR"),
            new("CFX"),
            new("CHR"),
            new("CHZ"),
            new("CKB"),
            new("CLV"),
            new("COCOS"),
            new("COMP"),
            new("COS"),
            new("COTI"),
            new("CRV"),
            new("CTK"),
            new("CTSI"),
            new("CTXC"),
            new("DAR"),
            new("DASH"),
            new("DEGO"),
            new("DENT"),
            new("DF"),
            new("DGB"),
            new("DIA"),
            new("DOCK"),
            new("DODO"),
            new("DOGE"),
            new("DOT"),
            new("DUSK"),
            new("DYDX"),
            new("EGLD"),
            new("ENJ"),
            new("ENS"),
            new("EOS"),
            new("EPX"),
            new("ETC"),
            new("ETH"),
            new("FET"),
            new("FIDA"),
            new("FIL"),
            new("FLOW"),
            new("FLUX"),
            new("FTM"),
            new("FXS"),
            new("GAL"),
            new("GALA"),
            new("GLMR"),
            new("GMX"),
            new("GRT"),
            new("GTC"),
            new("HARD"),
            new("HBAR"),
            new("HFT"),
            new("HIGH"),
            new("HIVE"),
            new("HOOK"),
            new("HOT"),
            new("ICP"),
            new("ICX"),
            new("IDEX"),
            new("IMX"),
            new("INJ"),
            new("IOST"),
            new("IOTA"),
            new("IOTX"),
            new("JASMY"),
            new("KAVA"),
            new("KDA"),
            new("KEY"),
            new("KLAY"),
            new("KNC"),
            new("KP3R"),
            new("KSM"),
            new("LAZIO"),
            new("LDO"),
            new("LEVER"),
            new("LINA"),
            new("LINK"),
            new("LIT"),
            new("LPT"),
            new("LQTY"),
            new("LRC"),
            new("LTC"),
            new("LTO"),
            new("LUNA"),
            new("LUNC"),
            new("MAGIC"),
            new("MANA"),
            new("MASK"),
            new("MATIC"),
            new("MBL"),
            new("MDT"),
            new("MDX"),
            new("MINA"),
            new("MKR"),
            new("MTL"),
            new("NEAR"),
            new("NEO"),
            new("NKN"),
            new("NMR"),
            new("NULS"),
            new("OCEAN"),
            new("OGN"),
            new("OM"),
            new("ONE"),
            new("ONT"),
            new("OP"),
            new("ORN"),
            new("OSMO"),
            new("OXT"),
            new("PAXG"),
            new("PEOPLE"),
            new("PERP"),
            new("PHA"),
            new("PHB"),
            new("POLYX"),
            new("POND"),
            new("PORTO"),
            new("PUNDIX"),
            new("PYR"),
            new("QI"),
            new("QNT"),
            new("QTUM"),
            new("QUICK"),
            new("REEF"),
            new("REI"),
            new("REN"),
            new("REQ"),
            new("RLC"),
            new("RNDR"),
            new("ROSE"),
            new("RSR"),
            new("RUNE"),
            new("RVN"),
            new("SAND"),
            new("SANTOS"),
            new("SC"),
            new("SFP"),
            new("SHIB"),
            new("SKL"),
            new("SLP"),
            new("SNX"),
            new("SOL"),
            new("SPELL"),
            new("STG"),
            new("STMX"),
            new("STORJ"),
            new("STX"),
            new("SUPER"),
            new("SUSHI"),
            new("SXP"),
            new("TFUEL"),
            new("THETA"),
            new("TLM"),
            new("TOMO"),
            new("TRB"),
            new("TROY"),
            new("TRX"),
            new("TVK"),
            new("TWT"),
            new("UMA"),
            new("UNFI"),
            new("UNI"),
            new("USTC"),
            new("UTK"),
            new("VET"),
            new("WAVES"),
            new("WAXP"),
            new("WING"),
            new("WOO"),
            new("XLM"),
            new("XMR"),
            new("XRP"),
            new("XTZ"),
            new("XVS"),
            new("YFI"),
            new("YGG"),
            new("ZEC"),
            new("ZEN"),
            new("ZIL"),
            new("ZRX"),
        };
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
                if (symbol == null)
					coinPrices.Remove(coin);
			}
		}

		public override string GetTradeLinkForCoin(CryptoCoin coin, TradeAction tradeAction)
		{
            if (tradeAction == TradeAction.Long)
                return $"https://www.binance.com/en/trade/{coin.Name}_USDT?type=spot";
			else
                return $"https://www.binance.com/en/trade/{coin.Name}_USDT?type=cross";
		}
	}
}
