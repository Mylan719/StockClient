namespace StockViewer.Fio.Data
{
    public class PortfolioDataRow
    {
        public string Name { get; set; }
        public string Symbol { get; set; }
        public decimal Amount { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? CloseChangePercentage { get; set; }
        public decimal? SharesTraded { get; set; }
        public decimal? CloseChangeByFio { get; set; }
        public decimal? ValueByFio { get; set; }
        public decimal? AmountBlockedInCdcp { get; set; }
        public override string ToString() => $"{Name} ({Symbol}): {Amount}{(UnitPrice != null ? $" rate {UnitPrice}" : "")}";
    }
}
