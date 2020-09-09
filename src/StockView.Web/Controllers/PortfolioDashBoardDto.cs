using StockViewer.Statistics.Data;

namespace StockView.Api.Controllers
{
    public class PortfolioDashBoardDto
    {
        public CurrencyInvestmentStatistic Statistic { get; set; }
        public InvestmentBalanceStatistic Balance { get; set; }
        public TotalInvestmentStatistic Total { get; set; }
    }
}