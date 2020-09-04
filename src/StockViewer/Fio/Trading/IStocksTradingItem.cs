namespace StockViewer.Fio.Trading
{
    public interface IStocksTradingItem : ITradingItem
    {
        string Symbol { get; set; }
    }
}
