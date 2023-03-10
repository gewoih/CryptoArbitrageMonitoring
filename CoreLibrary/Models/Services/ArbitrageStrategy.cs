using CoreLibrary.Models.Exchanges.Base;
using CoreLibrary.Models.Trading;

namespace CoreLibrary.Models.Services
{
    public sealed class ArbitrageStrategy
    {
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

				while (true)
				{
					try
					{
						var topChains = _arbitrageFinder.GetUpdatedChains(_minimumTotalDivergence);

						var chainsToRemove = arbitrageChainsDiscoveryTimes.Keys.Except(topChains).ToList();
						foreach (var chain in chainsToRemove)
						{
							arbitrageChainsDiscoveryTimes.Remove(chain);
                            Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss.fff}]: CHAIN REMOVED FROM POSSIBLE CHAINS FOR TRADES! {chain} {Environment.NewLine}");
						}

						foreach (var topChain in topChains)
						{
                            if (!arbitrageChainsDiscoveryTimes.ContainsKey(topChain))
                            {
                                Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss.fff}]: NEW CHAIN WAS DISCOVERED: {topChain}");
                                arbitrageChainsDiscoveryTimes[topChain] = DateTime.Now;
                            }

                            if (DateTime.Now - arbitrageChainsDiscoveryTimes[topChain] > TimeSpan.FromSeconds(_minimumSecondsOfChainHolding))
                            {
                                Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss.fff}]: CHAIN STAYED MORE THAN {_minimumSecondsOfChainHolding} SECONDS. TRYING OPEN THE TRADE.");
                                _tradesManager.TryOpenPositionByArbitrageChain(topChain);
								arbitrageChainsDiscoveryTimes.Remove(topChain);
							}
                            //else
                            //    Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss.fff}]: WAITING FOR CHAIN STAY MORE THAN " +
                            //        $"{_minimumSecondsOfChainHolding} SECONDS... {topChain} {Environment.NewLine}");
						}

                        await Task.Delay(1);
					}
					catch
					{
						continue;
					}
				}
			});
        }

        private void ArbitrageTradesManager_OnTradeOpened(ArbitrageTrade trade)
        {
            Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss.fff}]: OPENED TRADE {trade} {Environment.NewLine}");
        }

        private void ArbitrageTradesManager_OnTradeClosed(ArbitrageTrade trade)
        {
            Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss.fff}]: CLOSED TRADE {trade} {Environment.NewLine}");

            var arbitrageTradeProfitWithComission = (trade.LongTrade.Profit + trade.ShortTrade.Profit - trade.Comission) / 
                                                    (trade.LongTrade.EntryPrice + trade.ShortTrade.EntryPrice) 
                                                    * 100;

			File.AppendAllText($"Trades [" +
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
