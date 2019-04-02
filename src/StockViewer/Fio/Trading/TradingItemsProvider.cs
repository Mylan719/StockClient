using StockViewer.Fio.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockViewer.Fio.Trading
{
    public class TradingItemProvider
    {
        private readonly FioClient fioClient;

        public TradingItemProvider(FioClient fioClient)
        {
            this.fioClient = fioClient;
        }

        public async Task<IList<ITradingItem>> GetAllItemsAsync()
        {
            var tradeData = await fioClient.GetTradeDataAsync();

            return ProcessTradeData(tradeData);
        }


        private IList<ITradingItem> ProcessTradeData(List<TradeDataRow> tradeData)
        {
            var dividends = tradeData
                .Where(td => td.Price.HasValue && td.Amount.HasValue && string.IsNullOrWhiteSpace(td.Type))
                .Select(td => new Dividend {
                    Date = td.Date,
                    Currency = td.Currency,
                    Paied = td.Amount.Value,
                    Symbol = td.Symbol,
                    TransactionType = GetDividendTransactionType(td)
                });

            var trades = tradeData
                .Where(td => td.Price.HasValue && td.Amount.HasValue && !string.IsNullOrWhiteSpace(td.Type))
                .Select(td => new Trade
                {
                    Date = td.Date,
                    Fee = td.GetTradedValues().Item3,
                    Amount = td.Amount.Value,
                    Currency = td.Currency,
                    UnitPrice = td.Price.Value,
                    Paied = td.GetTradedValues().Item2,
                    Symbol = td.Symbol,
                    Type = GetTradeType(td)
                });

            //beter validating data
            var fees = tradeData
                  .Where(td => !td.Price.HasValue && td.GetTradedValues().Item3 == -td.GetTradedValues().Item2)
                  .Select(td => new Fee
                  {
                      Date = td.Date,
                      Currency = td.Currency,
                      Paied = td.GetTradedValues().Item3,
                  });

            //ensure values  are defined
            var transfers = tradeData
                  .Where(td => !td.Price.HasValue && td.GetTradedValues().Item3 == (decimal)0.0)
                  .Select(td => new Transfer
                  {
                      Date = td.Date,
                      Currency = td.Currency,
                      Paied = td.GetTradedValues().Item2,
                      Account = GetTransactionTypeAndAccount(td).Item2,
                      Type = GetTransactionTypeAndAccount(td).Item1
                  });

            return trades
                .OfType<ITradingItem>()
                .Concat(transfers)
                .Concat(fees)
                .Concat(dividends)
                .ToList();
        }

        private DividentTransactionType GetDividendTransactionType(TradeDataRow td)
        {
            if (string.IsNullOrWhiteSpace(td.Type))
            {
                var parts = td.Description.Split(" ");

                if (parts[0] != td.Symbol || parts[1] != "-")
                {
                    throw new InvalidOperationException("Unexpocted dividend description");
                }
                if (parts[2] == "Daň")
                {
                    return DividentTransactionType.Tax;
                }
                if (parts[2] == "Dividenda")
                {
                    return DividentTransactionType.Payment;
                }
            }
            throw new InvalidOperationException("Unexpocted dividend description");
        }

        private static TradeType GetTradeType(TradeDataRow td)
        {
            if (td.Type == "Nákup")
            {
                if (td.Description.StartsWith("NEPÁROVANÝ PŘEVOD"))
                {
                    return TradeType.TransferIn;
                }
                return TradeType.Buy;
            }
            if (td.Type == "Prodej")
            {
                if (td.Description.StartsWith("NEPÁROVANÝ PŘEVOD"))
                {
                    return TradeType.TransferOut;
                }
                return TradeType.Sell;
            }
            if (td.Type == "Převod mezi měnami")
            {
                return TradeType.Exchange;
            }
            throw new InvalidOperationException("Unexpected type ");
        }

        private static (TransferType, string) GetTransactionTypeAndAccount(TradeDataRow td)
        {
            var split = td.Description.Split(' ').ToList();
            var keyword = split.First();

            if (keyword == "Převod" && split.Count == 4)
            {
                return (TransferType.Out, split.Last());
            }
            if (keyword == "Vloženo")
            {
                return (TransferType.In, split[4]);
            }
            throw new InvalidOperationException($"Unrecognized description for suposed transfer <{td.Description}>");
        }
    }
}
