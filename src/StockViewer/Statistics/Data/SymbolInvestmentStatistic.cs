using System;
using System.Text;

namespace StockViewer.Statistics.Data
{

    public class SymbolInvestmentStatistic
    {
        public string Name { get; set; }
        public string Currency { get; set; }
        public decimal InvestedNowPrice { get; set; }
        public decimal InvestedNow { get; set; }
        public decimal Gain => InvestedNowPrice - InvestedNow + RealizedGains;
        public decimal GainPercentage => InvestedAllTime == decimal.Zero
            ? decimal.Zero
            : Gain / InvestedAllTime * 100;

        public decimal RealizedGains { get; internal set; }
        public decimal InvestedAllTime { get; internal set; }
        public decimal BasePrice { get; internal set; }

        public override string ToString() => $"{Name} ({Currency}): {InvestedNowPrice:N2} Gain: {Gain:N2} = {GainPercentage:N2}%";
    }
}
