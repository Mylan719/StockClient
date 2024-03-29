﻿using StockViewer.Fio.Data;
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
            //The table pages only one year back
            var tradeData = new List<TradeDataRow>();
            tradeData.AddRange(await fioClient.GetTradeDataAsync(DateTime.Now.AddYears(-1).AddDays(1), DateTime.Now));
            tradeData.AddRange(await fioClient.GetTradeDataAsync(DateTime.Now.AddYears(-2).AddDays(1), DateTime.Now.AddYears(-1)));
            tradeData.AddRange(await fioClient.GetTradeDataAsync(DateTime.Now.AddYears(-3).AddDays(1), DateTime.Now.AddYears(-2)));
            tradeData.AddRange(await fioClient.GetTradeDataAsync(DateTime.Now.AddYears(-4).AddDays(1), DateTime.Now.AddYears(-3)));
            tradeData.AddRange(await fioClient.GetTradeDataAsync(DateTime.Now.AddYears(-5).AddDays(1), DateTime.Now.AddYears(-4)));

            return ProcessTradeData(tradeData);
        }


        private IList<ITradingItem> ProcessTradeData(List<TradeDataRow> tradeData)
        {
            var spinOffs = tradeData
                .Where(td => 
                    td.Price.HasValue &&
                    td.Amount.HasValue &&
                    string.IsNullOrWhiteSpace(td.Type) &&
                    !string.IsNullOrWhiteSpace(td.Description) &&
                    td.Description.Contains("Spin-off"))
                .Select(td => new Trade
                {
                    Date = td.Date,
                    Fee = 0,
                    Amount = td.Amount.Value,
                    Currency = "USD",
                    UnitPrice = 0,
                    Paied = 0,
                    Symbol = td.Currency,
                    Type = TradeType.Buy
                }).ToList();

            var dividends = tradeData
                .Where(td =>
                          td.Price.HasValue &&
                          td.Amount.HasValue &&
                          string.IsNullOrWhiteSpace(td.Type) &&
                          (string.IsNullOrWhiteSpace(td.Description) || !td.Description.Contains("Spin-off")))
                .Select(td => new Dividend
                {
                    Date = td.Date,
                    Currency = td.Currency,
                    Paied = td.Amount.Value,
                    Symbol = td.Symbol,
                    TransactionType = GetDividendTransactionType(td)
                });

            var trades = tradeData
                .Where(td => td.Price.HasValue && td.Amount.HasValue && !string.IsNullOrWhiteSpace(td.Type) && !string.IsNullOrWhiteSpace(td.Currency))
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
                }).ToList();

            var currencyByStock = trades
                .GroupBy(k => k.Symbol, v => v.Currency)
                .ToDictionary(k => k.Key, v => v.FirstOrDefault());

            var stockSplitsAndCancellations = tradeData
                .Where(td => td.Price == 0.0m && td.Amount.HasValue && !string.IsNullOrWhiteSpace(td.Type) && string.IsNullOrWhiteSpace(td.Currency) && !string.IsNullOrWhiteSpace(td.Symbol))
                .Select(td => new Trade
                {
                    Date = td.Date,
                    Fee = 0,
                    Amount = td.Amount.Value,
                    Currency = currencyByStock[td.Symbol],
                    UnitPrice = td.Price.Value,
                    Paied = 0,
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
                .Concat(stockSplitsAndCancellations)
                .Concat(spinOffs)
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
                if (parts.LastOrDefault() == "Fee")
                {
                    return DividentTransactionType.Fee;
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

            if (keyword == "Převod" && (split.Count == 4 || split.Count == 5))
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
