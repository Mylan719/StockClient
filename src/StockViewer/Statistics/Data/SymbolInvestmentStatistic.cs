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
        public decimal TotalGain => MarketGain + RealizedGains;
        public decimal MarketGain => InvestedNowPrice - InvestedNow;
        public decimal TotalGainPercentage => InvestedAllTime == decimal.Zero
            ? decimal.Zero
            : TotalGain / InvestedAllTime * 100;

        public decimal MarketGainPercentage => InvestedAllTime == decimal.Zero
            ? decimal.Zero
            : MarketGain / InvestedAllTime * 100;

        public decimal RealizedGains { get; internal set; }
        public decimal InvestedAllTime { get; internal set; }

        public override string ToString() => $"{Name} ({Currency}): {InvestedNowPrice:N2} Gain: {TotalGain:N2} = {GainPercentage:N2}%";
    }
}
