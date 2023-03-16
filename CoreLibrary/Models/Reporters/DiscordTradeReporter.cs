using CoreLibrary.Models.Enums;
using CoreLibrary.Models.Trading;
using Discord;
using Discord.Rest;
using Discord.WebSocket;

namespace CoreLibrary.Models.Reporters
{
	public sealed class DiscordTradeReporter
	{
		private readonly DiscordSocketClient _discordClient;
		private readonly ulong _channelId;
		private ISocketMessageChannel _channel;
		private Dictionary<ArbitrageChain, RestUserMessage> _sendedSignalsByArbitrageChains;

		public DiscordTradeReporter(ulong channelId)
		{
			_channelId = channelId;
			_discordClient = new();
			_sendedSignalsByArbitrageChains = new();

			_discordClient.Log += _discordClient_Log;
			_discordClient.Ready += _discordClient_Ready;
			_discordClient.SlashCommandExecuted += _discordClient_SlashCommandExecuted;
		}

		private async Task _discordClient_Ready()
		{
			_channel = await _discordClient.GetChannelAsync(_channelId) as ISocketMessageChannel;
		}

		private Task _discordClient_SlashCommandExecuted(SocketSlashCommand arg)
		{
			throw new NotImplementedException();
		}

		private Task _discordClient_Log(LogMessage arg)
		{
			Console.WriteLine(arg.Message);
			return Task.CompletedTask;
		}

		public async Task InitializeAsync()
		{
			await _discordClient.LoginAsync(TokenType.Bot, "MTA4NTQ5Nzg1NzE4NTk1MTc1NA.GWuoAl.1WQVlJLBQddKNGjaDvlLvDnJ7v-c2UkqhfY_xc");
			await _discordClient.StartAsync();
		}

		public async Task SendSignalInfo(ArbitrageChain chain)
		{
			var message = await _channel.SendMessageAsync($"🟡",
				embed:
					new EmbedBuilder()
						.AddField("Coin:", chain.Coin.Name, true)
						.AddField("From:", $"[{chain.FromExchange.Name}]({chain.FromExchange.GetTradeLinkForCoin(chain.Coin, TradeAction.Long)})", true)
						.AddField("To:", $"[{chain.ToExchange.Name}]({chain.ToExchange.GetTradeLinkForCoin(chain.Coin, TradeAction.Short)})", true)
					.Build());

			_sendedSignalsByArbitrageChains[chain] = message;
		}

		public async Task SendOpenedTradeInfo(ArbitrageTrade trade)
		{
			await _channel.SendMessageAsync($"🟢",
				embed:
					new EmbedBuilder()
						.AddField("Coin:", trade.ArbitrageChain.Coin.Name)
						
						.AddField("From:", $"[{trade.ArbitrageChain.FromExchange.Name}]({trade.ArbitrageChain.FromExchange.GetTradeLinkForCoin(trade.ArbitrageChain.Coin, TradeAction.Long)})")
						.AddField("Amount:", $"{trade.LongTrade.Amount}шт.", true)
						.AddField("Avg. price:", $"{trade.LongTrade.EntryPrice}$", true)
						.AddField("Sum:", $"{trade.LongTrade.Amount * trade.LongTrade.EntryPrice}$", true)

						.AddField("To:", $"[{trade.ArbitrageChain.ToExchange.Name}]({trade.ArbitrageChain.ToExchange.GetTradeLinkForCoin(trade.ArbitrageChain.Coin, TradeAction.Short)})")
						.AddField("Amount:", $"{trade.ShortTrade.Amount}шт.", true)
						.AddField("Avg. price:", $"{trade.ShortTrade.EntryPrice}$", true)
						.AddField("Sum:", $"{trade.ShortTrade.Amount * trade.ShortTrade.EntryPrice}$", true)
					.Build());
		}

		public async Task SendClosedTradeInfo(ArbitrageTrade trade)
		{
			await _channel.SendMessageAsync($"🔴",
				embed:
					new EmbedBuilder()
						.AddField("Coin:", trade.ArbitrageChain.Coin.Name)

						.AddField("From:", $"[{trade.ArbitrageChain.FromExchange.Name}]({trade.ArbitrageChain.FromExchange.GetTradeLinkForCoin(trade.ArbitrageChain.Coin, TradeAction.Long)})")
						.AddField("Amount:", $"{trade.LongTrade.Amount}шт.", true)
						.AddField("Sum:", $"{trade.LongTrade.Amount * trade.LongTrade.EntryPrice}$", true)

						.AddField("To:", $"[{trade.ArbitrageChain.ToExchange.Name}]({trade.ArbitrageChain.ToExchange.GetTradeLinkForCoin(trade.ArbitrageChain.Coin, TradeAction.Short)})")
						.AddField("Amount:", $"{trade.ShortTrade.Amount}шт.", true)
						.AddField("Sum:", $"{trade.ShortTrade.Amount * trade.ShortTrade.EntryPrice}$", true)
					.Build());
		}

		public async Task StartReporting()
		{
			await _channel.SendMessageAsync($"New arbitrage strategy started!");
		}

		public async Task RemoveSignal(ArbitrageChain chain)
		{
			if (_sendedSignalsByArbitrageChains.TryGetValue(chain, out var message))
			{
				await _channel.DeleteMessageAsync(message.Id);
			}
		}
	}
}
