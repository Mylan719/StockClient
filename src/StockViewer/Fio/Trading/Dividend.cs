using System;

namespace StockViewer.Fio.Trading
{
    public class Dividend : ITradingItem
    {
        public DateTime Date { get; set; }
        public string Currency { get; set; }
        public decimal Paied { get; set; }
        public string Symbol { get; set; }
        public DividentTransactionType TransactionType { get; set; }
    }
}
