using ApiSharp.Helpers;
using CoreLibrary.Models.Enums;
using CoreLibrary.Models.Exchanges.Base;
using CoreLibrary.Models.Trading;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CoreLibrary.Models.Services
{
	public sealed class ArbitrageStrategy
	{
		private readonly TelegramBotClient _telegramBotClient;
		private readonly ChatId _telegramChatId = new("319784342");
		private readonly ArbitrageFinder _arbitrageFinder;
		private readonly ArbitrageTradesManager _tradesManager;
		private readonly decimal _minimumTotalDivergence;
		private readonly int _divergencePeriod;
		private readonly int _minimumSecondsInTrade;
		private readonly decimal _takeProfit;
		private readonly decimal _stopLoss;
		private readonly int _minimumSecondsOfChainHolding;

		public ArbitrageStrategy(List<CryptoCoin> coins, List<Exchange> exchanges, decimal minimumTotalDivergence, int divergencePeriod,
			int minimumSecondsInTrade, decimal takeProfit, decimal stopLoss, decimal amountPerTrade, int minimumSecondsOfChainHolding)
		{
			_telegramBotClient = new("6072726231:AAHs9iS_QJnbrOwiOhLb8PtqjAYBHDQvibI");
			_minimumTotalDivergence = minimumTotalDivergence;
			_divergencePeriod = divergencePeriod;
			_minimumSecondsInTrade = minimumSecondsInTrade;
			_takeProfit = takeProfit;
			_stopLoss = stopLoss;
			_minimumSecondsOfChainHolding = minimumSecondsOfChainHolding;

			_arbitrageFinder = new ArbitrageFinder(coins, exchanges, divergencePeriod, amountPerTrade);
			_tradesManager = new ArbitrageTradesManager(minimumSecondsInTrade, takeProfit, amountPerTrade, stopLoss);

			_tradesManager.OnTradeOpened += ArbitrageTradesManager_OnTradeOpened;
			_tradesManager.OnTradeClosed += ArbitrageTradesManager_OnTradeClosed;
		}

		public void Start()
		{
			Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss.fff}]: Starting arbitrage chains finder...");

			_ = Task.Run(async () =>
			{
				var arbitrageChainsDiscoveryTimes = new Dictionary<ArbitrageChain, DateTime>();
				var telegramMessagesByArbitrageChains = new Dictionary<ArbitrageChain, Message>();

				while (true)
				{
					try
					{
						var topChains = _arbitrageFinder.GetUpdatedChains(_minimumTotalDivergence);

						var chainsToRemove = arbitrageChainsDiscoveryTimes.Keys.Except(topChains).ToList();
						foreach (var chain in chainsToRemove)
						{
							arbitrageChainsDiscoveryTimes.Remove(chain);
							//if (telegramMessagesByArbitrageChains.TryGetValue(chain, out var message))
							//{
							//	_ = _telegramBotClient.DeleteMessageAsync(_telegramChatId, message.MessageId);
							//	telegramMessagesByArbitrageChains.Remove(chain);
							//}

							Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss.fff}]: CHAIN REMOVED FROM POSSIBLE CHAINS FOR TRADES! {chain} {Environment.NewLine}");
						}

						foreach (var topChain in topChains)
						{
							if (!arbitrageChainsDiscoveryTimes.ContainsKey(topChain) && !_tradesManager.IsAnyOpenedTradeForChain(topChain))
							{
								Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss.fff}]: NEW CHAIN WAS DISCOVERED: {topChain} {Environment.NewLine}");
								arbitrageChainsDiscoveryTimes[topChain] = DateTime.Now;

								//var sendedMessage = await _telegramBotClient.SendTextMessageAsync(
								//	chatId: _telegramChatId, 
								//	text: 
								//		$"🟡 {_minimumSecondsOfChainHolding} seconds " +
								//		$"[{topChain.Coin.Name}, " +
								//		$"<a href='{topChain.FromExchange.GetTradeLinkForCoin(topChain.Coin, TradeAction.Long)}'>{topChain.FromExchange.Name}</a>-" +
								//		$"{Environment.NewLine}" +
								//		$"<a href='{topChain.ToExchange.GetTradeLinkForCoin(topChain.Coin, TradeAction.Short)}'>{topChain.ToExchange.Name}</a>]", 
								//	parseMode: ParseMode.Html,
								//	disableWebPagePreview: true);

								//telegramMessagesByArbitrageChains[topChain] = sendedMessage;
							}
							else if (arbitrageChainsDiscoveryTimes.TryGetValue(topChain, out var chainDiscovered))
							{
								if (DateTime.Now - chainDiscovered < TimeSpan.FromSeconds(_minimumSecondsOfChainHolding))
									continue;

								Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss.fff}]: CHAIN STAYED MORE THAN " +
									$"{_minimumSecondsOfChainHolding} SECONDS. TRYING OPEN THE TRADE.");
								
								var newTrade = _tradesManager.TryOpenPositionByArbitrageChain(topChain);
								//_ = _telegramBotClient.DeleteMessageAsync(_telegramChatId, telegramMessagesByArbitrageChains[topChain].MessageId);
			
								arbitrageChainsDiscoveryTimes.Remove(topChain);
								//telegramMessagesByArbitrageChains.Remove(topChain);
							}
							else
							{
								//if (telegramMessagesByArbitrageChains.TryGetValue(topChain, out var message))
								//{
								//	var secondsLeftToOpenTrade = Math.Round(_minimumSecondsOfChainHolding - (DateTime.Now - arbitrageChainsDiscoveryTimes[topChain]).TotalSeconds, 2);
								//	var fromExchangeTradeLink = topChain.FromExchange.GetTradeLinkForCoin(topChain.Coin, TradeAction.Long);
								//	var toExchangeTradeLink = topChain.ToExchange.GetTradeLinkForCoin(topChain.Coin, TradeAction.Short);

								//	var newMessage = $"🟡 {secondsLeftToOpenTrade} seconds to go! " +
								//		$"[{topChain.Coin.Name}, " +
								//		$"<a href='{fromExchangeTradeLink}'>{topChain.FromExchange.Name}</a>-" +
								//		$"<a href='{toExchangeTradeLink}'>{topChain.ToExchange.Name}</a>]";

								//	if (newMessage.Trim() != message.Text.Trim())
								//	{
								//		_ = _telegramBotClient.EditMessageTextAsync(
								//				chatId: _telegramChatId,
								//				messageId: message.MessageId,
								//				text: newMessage,
								//				parseMode: ParseMode.Html,
								//				disableWebPagePreview: true);
								//	}
								//}
							}
						}

						await Task.Delay(1);
					}
					catch (Exception ex)
					{
						Console.WriteLine(ex);
						continue;
					}
				}
			});
		}

		private void ArbitrageTradesManager_OnTradeOpened(ArbitrageTrade trade)
		{
			var fromExchangeName = trade.ArbitrageChain.FromExchange.Name;
			var toExchangeName = trade.ArbitrageChain.ToExchange.Name;
			var fromExchangeTradeLink = trade.ArbitrageChain.FromExchange.GetTradeLinkForCoin(trade.ArbitrageChain.Coin, TradeAction.Long);
			var toExchangeTradeLink = trade.ArbitrageChain.ToExchange.GetTradeLinkForCoin(trade.ArbitrageChain.Coin, TradeAction.Short);
			var longPrice = trade.LongTrade.EntryPrice;
			var longAmount = trade.LongTrade.Amount;
			var shortPrice = trade.ShortTrade.EntryPrice;
			var shortAmount = trade.ShortTrade.Amount;

			//_ = _telegramBotClient.SendTextMessageAsync(_telegramChatId,
			//		$"🟢 " +
			//		$"[{trade.ArbitrageChain.Coin.Name}, " +
			//		$"<a href='{fromExchangeTradeLink}'>{fromExchangeName}</a>-" +
			//		$"<a href='{toExchangeTradeLink}'>{toExchangeName}</a>]" +
			//		$"{Environment.NewLine}" +
			//		$"{fromExchangeName}: {longAmount}шт/{Math.Round(longAmount * longPrice, 6).Normalize()}$ " +
			//		$"{Environment.NewLine}" +
			//		$"{toExchangeName}: {shortAmount}шт/{Math.Round(shortAmount * shortPrice, 6).Normalize()}$",
			//		ParseMode.Html,
			//		disableWebPagePreview: true);

			Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss.fff}]: OPENED TRADE {trade} {Environment.NewLine}");
		}

		private void ArbitrageTradesManager_OnTradeClosed(ArbitrageTrade trade)
		{
			var arbitrageTradeProfitWithComission = (trade.LongTrade.Profit + trade.ShortTrade.Profit - trade.Comission) /
													(trade.LongTrade.EntryPrice + trade.ShortTrade.EntryPrice)
													* 100;

			var fromExchangeTradeLink = trade.ArbitrageChain.FromExchange.GetTradeLinkForCoin(trade.ArbitrageChain.Coin, TradeAction.Long);
			var toExchangeTradeLink = trade.ArbitrageChain.ToExchange.GetTradeLinkForCoin(trade.ArbitrageChain.Coin, TradeAction.Short);

			var fromExchangeName = trade.ArbitrageChain.FromExchange.Name;
			var toExchangeName = trade.ArbitrageChain.ToExchange.Name;

			//_ = _telegramBotClient.SendTextMessageAsync(
			//		chatId: _telegramChatId,
			//		text: 
			//			$"🔴[{trade.ArbitrageChain.Coin.Name}, " +
			//			$"<a href='{fromExchangeTradeLink}'>{fromExchangeName}</a> {trade.LongTrade.Amount}шт. - " +
			//			$"<a href='{toExchangeTradeLink}'>{toExchangeName}</a> {trade.ShortTrade.Amount}шт.]",
			//		parseMode: ParseMode.Html,
			//		disableWebPagePreview: true);

			Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss.fff}]: CLOSED TRADE {trade} {Environment.NewLine}");
			
			System.IO.File.AppendAllText($"Trades [" +
					$"{_minimumTotalDivergence.ToString().Replace(".", ",")} " +
					$"{_divergencePeriod} " +
					$"{_minimumSecondsInTrade} " +
					$"{_minimumSecondsOfChainHolding} " +
					$"{_takeProfit.ToString().Replace(".", ",")} " +
					$"{_stopLoss.ToString().Replace(".", ",")}].txt",
				$"{trade.LongTrade.EntryDateTime};" +
				$"{trade.LongTrade.ExitDateTime};" +
				$"{(int)trade.TimeInTrade.TotalSeconds};" +
				$"{trade.ArbitrageChain.FromExchange.Name};" +
				$"{trade.ArbitrageChain.ToExchange.Name};" +
				$"{trade.ArbitrageChain.Coin.Name};" +
				$"{trade.LongTrade.EntryPrice};" +
				$"{trade.LongTrade.Amount};" +
				$"{trade.ShortTrade.EntryPrice};" +
				$"{trade.ShortTrade.Amount};" +
				$"{trade.LongTrade.ExitPrice};" +
				$"{trade.ShortTrade.ExitPrice};" +
				$"{trade.LongTrade.Profit};" +
				$"{trade.ShortTrade.Profit};" +
				$"{trade.LongTrade.Profit + trade.ShortTrade.Profit};" +
				$"{trade.Comission};" +
				$"{arbitrageTradeProfitWithComission}" +
				$"{Environment.NewLine}");
		}
	}
}
