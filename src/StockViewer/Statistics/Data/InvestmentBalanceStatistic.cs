using System.Collections.Generic;

namespace StockViewer.Statistics.Data
{
    public class InvestmentBalanceStatistic
    {
        public decimal TotalInvestment { get; internal set; }
        public Dictionary<string, decimal> SymbolPercentages { get; internal set; }
    }
}