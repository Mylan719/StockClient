namespace StockViewer.Statistics.Data
{
    public class TotalInvestmentStatistic
    {
        public decimal AllTimeInvested { get; set; }
        public decimal AllTimeInvestmentValue { get; set; }
        public decimal Gain => AllTimeInvestmentValue - AllTimeInvested;
        public decimal GainPercentage => AllTimeInvested == decimal.Zero
            ? decimal.Zero
            : Gain / AllTimeInvested * 100;

        public override string ToString() => $"{AllTimeInvested:N2} invested, now at {AllTimeInvestmentValue:N2}, Gain: {Gain:N2} = {GainPercentage:N2}%";
    }
}
