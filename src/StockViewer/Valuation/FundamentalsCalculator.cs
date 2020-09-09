using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using System.Linq;

namespace StockViewer.BL.Valuation
{
    public class FundamentalsCalculator
    {
        public GrowthModel CalculateEquityGrowth(string symbol)
        {
            var model = GetData(symbol);
            return new GrowthModel
            {
                Years = model.Years,
                Growth = model.Equity
                    .Select((equity, yearsBack) => GrowthRate(equity, model.Equity[0], yearsBack))
                    .ToList()
            };
        }

        public GrowthModel CalculateRevenueGrowth(string symbol)
        {
            var model = GetData(symbol);
            return new GrowthModel
            {
                Years = model.Years,
                Growth = model.Revenue
                    .Select((equity, yearsBack) => GrowthRate(equity, model.Revenue[0], yearsBack))
                    .ToList()
            };
        }

        public FundamentalsModel GetFundamentals(string symbol)
        {
            try
            {
                return GetData(symbol);
            }catch(IOException)
            {
                return null;
            }
        }

        public GrowthModel CalculateNetIncomeGrowth(string symbol)
        {
            var model = GetData(symbol);
            return new GrowthModel
            {
                Years = model.Years,
                Growth = model.NetIncome
                   .Select((equity, yearsBack) => GrowthRate(equity, model.NetIncome[0], yearsBack))
                   .ToList()
            };
        }

        public BasicRevenueValuationModel ValuationFromSP(string symbol)
        {
            var model = GetData(symbol);

            var averagePS = (model.PriceToSalesMax + model.PriceToSalesMin) / 2;
            var growth5years = GrowthRate(model.Revenue[4], model.Revenue[0], 4);
            var revenuePerShare = model.RevenuePerShare(0);

            var futurePrice = Math.Floor( (double) revenuePerShare * Math.Pow(1.0 + (double)growth5years, 5) * (double)averagePS);

            return new BasicRevenueValuationModel{
                AveragePS = averagePS,
                RevenuePerShare = revenuePerShare,
                Growth = growth5years,
                FuturePrice = futurePrice
            };
        }

        public BasicValuationModel ValuationFromPE(string symbol)
        {
            var model = GetData(symbol);

            var averagePE = (model.PriceToEarningsMax + model.PriceToEarningsMin) / 2;
            var growth5years = GrowthRate(model.Equity[4], model.Equity[0], 4);

            var growthBasedPE = Math.Ceiling(growth5years * 100) * 2;

            var adjustedPE = growthBasedPE < averagePE ? growthBasedPE : averagePE;

            var futurePrice = Math.Floor((double)model.EPS[0] * Math.Pow(1.0 + (double)growth5years, 5) * (double)adjustedPE);

            return new BasicValuationModel { 
                AveragePE = averagePE,
                AdjustedPE = adjustedPE,
                Growth = growth5years,
                ESP = model.EPS[0],
                FuturePrice = futurePrice
            };
        }

        private decimal GrowthRate(decimal presentValue, decimal futureValue, int years) =>
            (decimal)Math.Pow((double)(futureValue / presentValue), (1 / (double)years))-1;

        private FundamentalsModel GetData(string symbol)
        {
            using (Stream stream = GetType().Assembly.GetManifestResourceStream($"StockViewer.BL.Valuation.Data.{symbol}.json"))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    var json = reader.ReadToEnd();
                    return JsonConvert.DeserializeObject<FundamentalsModel>(json);
                }
            }
        }
    }
}
