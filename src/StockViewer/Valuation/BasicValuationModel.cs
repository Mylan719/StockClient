namespace StockViewer.BL.Valuation
{
    public class BasicRevenueValuationModel
    {
        public decimal AveragePS { get; internal set; }
        public decimal Growth { get; internal set; }
        public double FuturePrice { get; internal set; }
        public decimal RevenuePerShare { get; internal set; }
    }
}