using System.Collections.Generic;

namespace StockViewer.Statistics.Data
{
    public class CurrencyInvestmentStatistic
    {
        public string Currency { get; set; }
        public List<SymbolInvestmentStatistic> Symbols { get; set; }
        public decimal Fees { get; set; }
        public decimal Cash { get; set; }
    }
}
