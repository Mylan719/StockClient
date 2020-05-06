namespace StockViewer.Statistics.Data
{
    public class TotalInvestmentStatistic
    {
        public decimal InvestedAllTime { get; set; }
        public decimal InvestedNowPrice { get; set; }
        public decimal RealizedGains { get; set; }
        public decimal InvestedNow { get; set; }
        public decimal Gain => InvestedNowPrice - InvestedNow + RealizedGains;
        public decimal GainPercentage => InvestedAllTime == decimal.Zero
            ? decimal.Zero
            : Gain / InvestedAllTime * 100;

        public override string ToString() => $"{InvestedAllTime:N2} invested, Gain: {Gain:N2} = {GainPercentage:N2}%";
    }
}
