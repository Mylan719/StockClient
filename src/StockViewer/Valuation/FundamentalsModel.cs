using System;
using System.Collections.Generic;
using System.Text;

namespace StockViewer.BL.Valuation
{
    public class FundamentalsModel
    {
        public List<int> Years { get; set; }
        public List<decimal> Equity { get; set; }
        public List<decimal> Revenue { get; set; }
        public List<decimal> NetIncome { get; set; }
        public List<decimal> EPS { get; set; }
        public List<decimal> SharesOutstanding { get; set; }
        public decimal PriceToSalesMin { get; set; }
        public decimal PriceToSalesMax { get; set; }
        public decimal PriceToEarningsMin { get; set; }
        public decimal PriceToEarningsMax { get; set; }

        public decimal RevenuePerShare(int year) => Revenue[year] / SharesOutstanding[year];
    }

    public class GrowthModel
    {
        public List<decimal> Growth { get; set; }
        public List<int> Years { get; set; }
    }

}
