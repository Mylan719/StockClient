using System.Collections.Generic;

namespace StockViewer.Statistics.Data
{
    public class InvestmentBalanceStatistic
    {
        public decimal TotalInvestment { get; set; }
        public IList<SymbolBalance> Symbols { get; set; }
    }
}