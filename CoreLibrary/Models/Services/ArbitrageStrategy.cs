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
		private readonly Dictionary<ArbitrageChain, DateTime> _arbitrageChainsDiscoveryTimes;
		private readonly Dictionary<ArbitrageChain, Message> _arbitrageChainsTelegramMessages;

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
			_arbitrageChainsTelegramMessages = new();
			_arbitrageChainsDiscoveryTimes = new();

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
				while (true)
				{
					try
					{
						var chains = await _arbitrageFinder.GetUpdatedChains(_minimumTotalDivergence);
						RemoveIrrelevantChains(chains);

						foreach (var chain in chains)
						{
							await NotifyIfNewChainDiscovered(chain);
							TryOpenTrade(chain);
							TryUpdateTelegramSignal(chain);
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

		private void TryUpdateTelegramSignal(ArbitrageChain chain)
		{
			if (_arbitrageChainsTelegramMessages.TryGetValue(chain, out var message))
			{
				var secondsLeftToOpenTrade = Math.Round(_minimumSecondsOfChainHolding - (DateTime.Now - _arbitrageChainsDiscoveryTimes[chain]).TotalSeconds, 2);
				var fromExchangeTradeLink = chain.FromExchange.GetTradeLinkForCoin(chain.Coin, TradeAction.Long);
				var toExchangeTradeLink = chain.ToExchange.GetTradeLinkForCoin(chain.Coin, TradeAction.Short);

				var newMessage = $"🟡 {secondsLeftToOpenTrade} seconds to go! " +
					$"[{chain.Coin.Name}, " +
					$"<a href='{fromExchangeTradeLink}'>{chain.FromExchange.Name}</a>-" +
					$"<a href='{toExchangeTradeLink}'>{chain.ToExchange.Name}</a>]";

				if (newMessage.Trim() != message.Text.Trim())
				{
					_ = _telegramBotClient.EditMessageTextAsync(
							chatId: _telegramChatId,
							messageId: message.MessageId,
							text: newMessage,
							parseMode: ParseMode.Html,
							disableWebPagePreview: true);
				}
			}
		}

		private bool TryOpenTrade(ArbitrageChain chain)
		{
			if (_arbitrageChainsDiscoveryTimes.TryGetValue(chain, out var chainDiscovered))
			{
				if (DateTime.Now - chainDiscovered < TimeSpan.FromSeconds(_minimumSecondsOfChainHolding))
					return false;

				Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss.fff}]: CHAIN STAYED MORE THAN " +
					$"{_minimumSecondsOfChainHolding} SECONDS. TRYING OPEN THE TRADE.");

				var newTrade = _tradesManager.TryOpenTradeByArbitrageChain(chain);
				_ = _telegramBotClient.DeleteMessageAsync(_telegramChatId, _arbitrageChainsTelegramMessages[chain].MessageId);

				_arbitrageChainsDiscoveryTimes.Remove(chain);
				_arbitrageChainsTelegramMessages.Remove(chain);

				return newTrade is not null;
			}

			return false;
		}

		private async Task<bool> NotifyIfNewChainDiscovered(ArbitrageChain chain)
		{
			if (!_arbitrageChainsDiscoveryTimes.ContainsKey(chain) && !_tradesManager.IsAnyOpenedTradeForChain(chain))
			{
				Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss.fff}]: NEW CHAIN WAS DISCOVERED: {chain} {Environment.NewLine}");
				_arbitrageChainsDiscoveryTimes[chain] = DateTime.Now;

				var sendedMessage = await _telegramBotClient.SendTextMessageAsync(
					chatId: _telegramChatId,
					text:
						$"🟡 {_minimumSecondsOfChainHolding} seconds " +
						$"[{chain.Coin.Name}, " +
						$"<a href='{chain.FromExchange.GetTradeLinkForCoin(chain.Coin, TradeAction.Long)}'>{chain.FromExchange.Name}</a>-" +
						$"{Environment.NewLine}" +
						$"<a href='{chain.ToExchange.GetTradeLinkForCoin(chain.Coin, TradeAction.Short)}'>{chain.ToExchange.Name}</a>]",
					parseMode: ParseMode.Html,
					disableWebPagePreview: true);

				_arbitrageChainsTelegramMessages[chain] = sendedMessage;

				return true;
			}

			return false;
		}

		private void RemoveIrrelevantChains(IEnumerable<ArbitrageChain> chains)
		{
			var chainsToRemove = _arbitrageChainsDiscoveryTimes.Keys.Except(chains).ToList();
			foreach (var chain in chainsToRemove)
			{
				_arbitrageChainsDiscoveryTimes.Remove(chain);
				if (_arbitrageChainsTelegramMessages.TryGetValue(chain, out var message))
				{
					_ = _telegramBotClient.DeleteMessageAsync(_telegramChatId, message.MessageId);
					_arbitrageChainsTelegramMessages.Remove(chain);
				}

				Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss.fff}]: CHAIN REMOVED FROM POSSIBLE CHAINS FOR TRADES! {chain} {Environment.NewLine}");
			}
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

			_ = _telegramBotClient.SendTextMessageAsync(_telegramChatId,
					$"🟢 " +
					$"[{trade.ArbitrageChain.Coin.Name}, " +
					$"<a href='{fromExchangeTradeLink}'>{fromExchangeName}</a>-" +
					$"<a href='{toExchangeTradeLink}'>{toExchangeName}</a>]" +
					$"{Environment.NewLine}" +
					$"{fromExchangeName}: {longAmount}шт/{Math.Round(longAmount * longPrice, 6).Normalize()}$ " +
					$"{Environment.NewLine}" +
					$"{toExchangeName}: {shortAmount}шт/{Math.Round(shortAmount * shortPrice, 6).Normalize()}$",
					ParseMode.Html,
					disableWebPagePreview: true);

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

			_ = _telegramBotClient.SendTextMessageAsync(
					chatId: _telegramChatId,
					text: 
						$"🔴[{trade.ArbitrageChain.Coin.Name}, " +
						$"<a href='{fromExchangeTradeLink}'>{fromExchangeName}</a> {trade.LongTrade.Amount}шт. - " +
						$"<a href='{toExchangeTradeLink}'>{toExchangeName}</a> {trade.ShortTrade.Amount}шт.]",
					parseMode: ParseMode.Html,
					disableWebPagePreview: true);

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
