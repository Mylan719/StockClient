using System;

namespace StockViewer.Fio.Trading
{
    public class Trade : ITradingItem
    {
        public DateTime Date { get; set; }
        public string Currency { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Paied { get; set; }
        public decimal Amount { get; set; }
        public decimal Fee { get; set; }
        public string Symbol { get; set; }
        public TradeType Type { get; set; }

        public bool IsAcquire => Type == TradeType.Buy || Type == TradeType.TransferIn;
        public bool IsDispose => Type == TradeType.Sell || Type == TradeType.TransferOut;
        public bool IsShareTrade => IsAcquire || IsDispose;
        public bool IsMoneyTrade => Type == TradeType.Exchange;

        public override string ToString()
        {
            return $"{Type} - {Date} - {Symbol}: {UnitPrice}, {Paied}({Fee})";
        }

        public Trade Copy(decimal amount)
        {
            return new Trade
            {
                Date = Date,
                Currency = Currency,
                UnitPrice = UnitPrice,
                Paied = Paied,
                Amount = amount,
                Fee = Fee,
                Symbol = Symbol,
                Type = Type
            };
        }
    }
}
