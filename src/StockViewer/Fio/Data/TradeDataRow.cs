using System;
using System.Collections.Generic;
using System.Linq;

namespace StockViewer.Fio.Data
{
    //Datum obchodu 	Směr 	Symbol 	Cena 	Počet 	Měna 	Objem v CZK 	Poplatky v CZK 	Objem v USD 	Poplatky v USD 	Objem v EUR 	Poplatky v EUR 	Text FIO
    public class TradeDataRow
    {
        public DateTime Date { get; set; }
        public string Type { get; set; }
        public string Symbol { get; set; }
        public decimal? Price { get; set; }
        public decimal? Amount { get; set; }
        public string Currency { get; set; }
        public decimal? ValueCzk { get; set; }
        public decimal? FeesCzk { get; set; }
        public decimal? ValueUsd { get; set; }
        public decimal? FeesUsd { get; set; }
        public decimal? ValueEur { get; set; }
        public decimal? FeesEur { get; set; }
        public string Description { get; set; }

        public (string, decimal, decimal) GetTradedValues()
        {
            var stuff = new[]{
                 new { Currency="CZK", Value = ValueCzk, Fee=FeesCzk },
                 new { Currency="USD", Value = ValueUsd, Fee=FeesUsd},
                 new { Currency="EUR", Value = ValueEur, Fee=FeesEur }
            };

            var defined = stuff.FirstOrDefault(v => v.Value.HasValue && v.Fee.HasValue)
                ?? throw new InvalidOperationException("Columns with fees and total values are probably incomplete.");

            return (defined.Currency, defined.Value.Value, defined.Fee.Value);

        }

        public override string ToString() => $"{Date} - {Symbol} - {(Price?.ToString() ?? "")} - {(Amount?.ToString() ?? "")}";
    }
}
