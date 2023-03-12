using CoreLibrary.Models.Enums;
using CoreLibrary.Models.Exchanges.Base;
using Kucoin.Net.Clients;

namespace CoreLibrary.Models.Exchanges
{
	public sealed class KucoinExchange : Exchange
	{
		public override string Name => "Kucoin";
		public override TickersInfo TickersInfo => new("-", CaseType.Uppercase, new("USDT"));
		public override List<CryptoCoin> MarginCoins => new()
		{
			new("1INCH"),
			new("ADA"),
			new("AGIX"),
			new("AIOZ"),
			new("ALGO"),
			new("ALICE"),
			new("ANC"),
			new("ANKR"),
			new("ANT"),
			new("APE"),
			new("API3"),
			new("APT"),
			new("AR"),
			new("ARPA"),
			new("ATOM"),
			new("AUDIO"),
			new("AVAX"),
			new("AXS"),
			new("BAT"),
			new("BCH"),
			new("BCHSV"),
			new("BLUR"),
			new("BNB"),
			new("BTC"),
			new("BTT"),
			new("C98"),
			new("CAKE"),
			new("CELO"),
			new("CHR"),
			new("CHZ"),
			new("CKB"),
			new("CLV"),
			new("COMP"),
			new("CRO"),
			new("CRV"),
			new("CTSI"),
			new("DAO"),
			new("DAR"),
			new("DASH"),
			new("DGB"),
			new("DODO"),
			new("DOGE"),
			new("DOT"),
			new("DYDX"),
			new("EGLD"),
			new("ELON"),
			new("ENJ"),
			new("ENS"),
			new("EOS"),
			new("ERN"),
			new("ETC"),
			new("ETH"),
			new("FIL"),
			new("FITFI"),
			new("FLOW"),
			new("FLUX"),
			new("FRONT"),
			new("FTM"),
			new("FXS"),
			new("GAL"),
			new("GLMR"),
			new("GRT"),
			new("HBAR"),
			new("ICP"),
			new("ILV"),
			new("IMX"),
			new("INJ"),
			new("IOST"),
			new("IOTA"),
			new("IOTX"),
			new("JASMY"),
			new("JST"),
			new("KAVA"),
			new("KDA"),
			new("KSM"),
			new("LINA"),
			new("LINK"),
			new("LPT"),
			new("LRC"),
			new("LTC"),
			new("LTO"),
			new("MANA"),
			new("MASK"),
			new("MATIC"),
			new("MKR"),
			new("MOVR"),
			new("MXC"),
			new("NEAR"),
			new("NEO"),
			new("NFT"),
			new("NKN"),
			new("OCEAN"),
			new("OGN"),
			new("OMG"),
			new("ONE"),
			new("OP"),
			new("ORN"),
			new("PEOPLE"),
			new("POLS"),
			new("POND"),
			new("PYR"),
			new("QI"),
			new("QNT"),
			new("REN"),
			new("REQ"),
			new("RLC"),
			new("RNDR"),
			new("ROSE"),
			new("RSR"),
			new("RUNE"),
			new("SAND"),
			new("SHIB"),
			new("SKL"),
			new("SLP"),
			new("SNX"),
			new("SOL"),
			new("SOS"),
			new("STORJ"),
			new("STX"),
			new("SUN"),
			new("SUPER"),
			new("SUSHI"),
			new("SXP"),
			new("THETA"),
			new("TLM"),
			new("TRX"),
			new("UMA"),
			new("UNI"),
			new("USDC"),
			new("USDD"),
			new("VELO"),
			new("VET"),
			new("VRA"),
			new("WAVES"),
			new("WAX"),
			new("WIN"),
			new("WOO"),
			new("XEM"),
			new("XLM"),
			new("XMR"),
			new("XPR"),
			new("XRP"),
			new("XTZ"),
			new("YFI"),
			new("YGG"),
			new("ZEC"),
			new("ZIL"),
		};
		private readonly KucoinSocketClient _socketClient = new();
		private readonly KucoinClient _client = new();

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

		protected override async Task RemoveNonExistentCoins()
		{
			var result = await _client.SpotApi.ExchangeData.GetSymbolsAsync();
			foreach (var coin in coinPrices.Keys.ToList())
			{
				var symbol = result.Data.FirstOrDefault(d => d.Symbol == GetTickerByCoin(coin));

				if (symbol is null)
					coinPrices.Remove(coin);
			}
		}

		public override string GetTradeLinkForCoin(CryptoCoin coin, TradeAction tradeAction)
		{
			return $"https://www.kucoin.com/trade/{coin.Name}-USDT";
		}
	}
}
