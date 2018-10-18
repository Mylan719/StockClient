using System;

namespace StockViewer.Fio.Trading
{
    public class Fee : ITradingItem
    {
        public DateTime Date { get; set; }
        public string Currency { get; set; }
        public decimal Paied { get; set; }
        public override string ToString()
        {
            return $"{Date} - {Paied},- {Currency}";
        }
    }
}
