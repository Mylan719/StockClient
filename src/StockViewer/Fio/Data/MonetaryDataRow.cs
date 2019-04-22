using System;
using System.Collections.Generic;
using System.Text;

namespace StockViewer.Fio.Data
{
    public class MonetaryDataRow
    {
        public string Currency { get; set; }
        public decimal? Total { get; set; }
        public decimal? InTransfer { get; set; }
        public decimal? InSales { get; set; }
        public decimal? InBuys { get; set; }
        public decimal BuyingPower { get; set; }
        public decimal ToWithdraw { get; set; }
        public override string ToString() => $"{BuyingPower}({Total}) {Currency}";
    }
}
