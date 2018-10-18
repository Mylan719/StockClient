using System;

namespace StockViewer.Fio.Trading
{
    public class Transfer : ITradingItem
    {
        public DateTime Date { get; set; }
        public string Currency { get; set; }
        public decimal Paied { get; set; }
        public string Account { get; set; }
        public TransferType Type { get; set; }

        public override string ToString()
        {
            return $"{Type} - {Date} - {Account}: {Paied},- {Currency}";
        }
    }
}
