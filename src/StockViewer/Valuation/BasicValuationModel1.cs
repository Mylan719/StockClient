namespace StockViewer.BL.Valuation
{
    public class BasicValuationModel
    {
        public decimal AveragePE { get; internal set; }
        public decimal AdjustedPE { get; internal set; }
        public decimal Growth { get; internal set; }
        public decimal ESP { get; internal set; }
        public double FuturePrice { get; internal set; }
    }
}