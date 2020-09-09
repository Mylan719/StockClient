using Microsoft.AspNetCore.Mvc;
using StockViewer.BL.Valuation;

namespace StockView.Api.Controllers
{
    [Route("api/[controller]")]
    public class FundamentalsController
    {
        private readonly FundamentalsCalculator fundamentalsCalculator;

        public FundamentalsController(FundamentalsCalculator fundamentalsCalculator)
        {
            this.fundamentalsCalculator = fundamentalsCalculator;
        }

        [Route("{symbol}")]
        public FundamentalsModel Get(string symbol)
        {
            return fundamentalsCalculator.GetFundamentals(symbol);
        }


        [Route("{symbol}/growth/equity")]
        public GrowthModel GetEquityGrowth(string symbol)
        {
            return fundamentalsCalculator.CalculateEquityGrowth(symbol);
        }

        [Route("{symbol}/growth/revenue")]
        public GrowthModel GetRevenueGrowth(string symbol)
        {
            return fundamentalsCalculator.CalculateRevenueGrowth(symbol);
        }

        [Route("{symbol}/growth/net-income")]
        public GrowthModel GetGrowth(string symbol)
        {
            return fundamentalsCalculator.CalculateNetIncomeGrowth(symbol);
        }

        [Route("{symbol}/valuation-pe")]
        public BasicValuationModel FuturePricePE(string symbol)
        {
            return fundamentalsCalculator.ValuationFromPE(symbol);
        }

        [Route("{symbol}/valuation-ps")]
        public BasicRevenueValuationModel FuturePriceSP(string symbol)
        {
            return fundamentalsCalculator.ValuationFromSP(symbol);
        }
    }
}