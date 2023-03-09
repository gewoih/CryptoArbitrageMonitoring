using CoreLibrary.Models.Enums;
using System.Collections.Concurrent;

namespace CoreLibrary.Models.MarketInfo
{
	public sealed class MarketData
	{
		private ConcurrentDictionary<decimal, decimal> _bids;
		private ConcurrentDictionary<decimal, decimal> _asks;
		public readonly ConcurrentBag<Tick> Ticks;
		public Tick Last => Ticks.Any() ? Ticks.MaxBy(t => t.Ticks) : new(0, 0);
		public DateTime LastTradeDateTime => Ticks.Any() ? Ticks.MaxBy(t => t.Ticks).DateTime : DateTime.MinValue;
		public decimal Ask => _asks.Any() ? _asks.Min(a => a.Key) : 0;
		public decimal Bid => _bids.Any() ? _bids.Max(bid => bid.Key) : 0;
		public decimal Spread => Ask != 0 && Bid != 0 ? Math.Abs(Bid / Ask * 100 - 100) : 0;
		public MarketData()
		{
			Ticks = new();
			_bids = new();
			_asks = new();
		}

		public bool AddTick(decimal lastPrice, DateTime time, int tradeNumber = -1)
		{
			if (Ticks.FirstOrDefault(t => t.Ticks == time.Ticks) is null)
			{
				if (tradeNumber != -1)
				{
					if (Ticks.FirstOrDefault(t => t.Number == tradeNumber) is null)
					{
						Ticks.Add(new(tradeNumber, lastPrice, time.Ticks));
						return true;
					}
					else
						return false;
				}
				else
				{
					Ticks.Add(new(lastPrice, time.Ticks));
					return true;
				}
			}

			return false;
		}

		public void UpdateOrderBook(IEnumerable<KeyValuePair<decimal, decimal>> bids, IEnumerable<KeyValuePair<decimal, decimal>> asks, bool isFullOrderBook = false)
		{
			if (isFullOrderBook)
			{
				_bids = new ConcurrentDictionary<decimal, decimal>(bids);
				_asks = new ConcurrentDictionary<decimal, decimal>(asks);
				return;
			}

            foreach (var bid in bids)
            {
				if (bid.Value == 0)
					_bids.TryRemove(bid.Key, out decimal value);
				else
					_bids[bid.Key] = bid.Value;
            }

            foreach (var ask in asks)
            {
                if (ask.Value == 0)
                    _asks.TryRemove(ask.Key, out decimal value);
                else
                    _asks[ask.Key] = ask.Value;
            }
        }

		public decimal GetSMA(int period)
		{
			if (period <= 0)
				throw new ArgumentOutOfRangeException(nameof(period));

			if (Ticks.Count < period)
				return 0;

			return Ticks
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
