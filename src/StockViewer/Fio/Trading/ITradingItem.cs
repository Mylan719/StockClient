using System;

namespace StockViewer.Fio.Trading
{
    public interface ITradingItem
    {
        DateTime Date { get; set; }
        string Currency { get; set; }
        decimal Paied { get; set; }
    }
}
