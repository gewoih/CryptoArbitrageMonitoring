using CoreLibrary.Models.Enums;
using CoreLibrary.Models.Exchanges.Base;
using System.Collections.Concurrent;

namespace CoreLibrary.Models.MarketInfo
{
	public sealed class MarketData
	{
		private Exchange _exchange;
		private CryptoCoin _coin;
		private ConcurrentDictionary<decimal, decimal> _bids;
		private ConcurrentDictionary<decimal, decimal> _asks;
		private readonly ConcurrentBag<Tick> _ticks;
		public Tick Last = new(0, 0);
		public decimal Ask = 0;
		public decimal Bid = 0;
		public decimal Spread = 0;

		public MarketData(Exchange exchange, CryptoCoin coin)
		{
			_ticks = new();
			_bids = new();
			_asks = new();
			_exchange = exchange;
			_coin = coin;
		}

		public void ClearOrderBook()
		{
			_bids.Clear();
			_asks.Clear();
		}

		public bool AddTick(decimal lastPrice, DateTime time, int tradeNumber = -1)
		{
			if (_ticks.FirstOrDefault(t => t.Ticks == time.Ticks) is null)
			{
				if (tradeNumber != -1)
				{
					if (_ticks.FirstOrDefault(t => t.Number == tradeNumber) is not null)
						return false;

					var newTick = new Tick(tradeNumber, lastPrice, time.Ticks);
					_ticks.Add(newTick);
					Last = newTick;
				}
				else
				{
					var newTick = new Tick(lastPrice, time.Ticks);
					_ticks.Add(newTick);
					Last = newTick;
				}
				
				return true;
			}

			return false;
		}

		public void UpdateOrderBook(IEnumerable<KeyValuePair<decimal, decimal>> bids, IEnumerable<KeyValuePair<decimal, decimal>> asks, bool isFullOrderBook = false)
		{
			if (isFullOrderBook)
			{
				_bids = new ConcurrentDictionary<decimal, decimal>(bids.DistinctBy(b => b.Key));
				_asks = new ConcurrentDictionary<decimal, decimal>(asks.DistinctBy(a => a.Key));
			}
			else
			{
				foreach (var bid in bids)
				{
					if (bid.Value == 0)
						_bids.TryRemove(bid.Key, out _);
					else
						_bids[bid.Key] = bid.Value;
				}

				foreach (var ask in asks)
				{
					if (ask.Value == 0)
						_asks.TryRemove(ask.Key, out _);
					else
						_asks[ask.Key] = ask.Value;
				}
			}

			Ask = _asks.Any() ? _asks.Min(a => a.Key) : 0;
			Bid = _bids.Any() ? _bids.Max(bid => bid.Key) : 0;
			Spread = Ask != 0 && Bid != 0 ? Math.Abs(Bid / Ask * 100 - 100) : 0;
		}

		public decimal GetSMA(int period)
		{
			if (period <= 0)
				throw new ArgumentOutOfRangeException(nameof(period));

			if (_ticks.Count < period)
				return 0;

			return _ticks
					.OrderByDescending(t => t.Ticks)
					.Take(period)
					.Average(t => t.Price);
		}

		public decimal GetAverageMarketPriceForAmount(decimal amount, TradeAction tradeAction)
		{
			var totalPrice = 0m;
			var totalAmount = 0m;
			var remainingAmount = amount;

			var orders = tradeAction == TradeAction.Long ?  _asks.ToArray().OrderBy(o => o.Key) : _bids.ToArray().OrderByDescending(o => o.Key);

			foreach (var order in orders)
			{
				var price = order.Key;
				var orderAmount = order.Value;

				if (orderAmount * price < remainingAmount)
				{
					totalPrice += orderAmount * price;
					totalAmount += orderAmount;
					remainingAmount -= orderAmount * price;
				}
				else
				{
					totalPrice += remainingAmount;
					totalAmount += remainingAmount / price;
					remainingAmount = 0;
					break;
				}
			}

			if (totalAmount == 0 || totalPrice < amount)
				return 0;

			var averagePrice = totalPrice / totalAmount;

			if (amount < averagePrice)
				return 0;

			return averagePrice;
		}
	}
}
