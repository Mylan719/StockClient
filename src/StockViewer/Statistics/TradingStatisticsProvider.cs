using StockViewer.Fio;
using StockViewer.Fio.Data;
using StockViewer.Fio.Trading;
using StockViewer.Statistics.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StockViewer.Statistics
{
    public class TradingStatisticsProvider
    {
        public List<CurrencyInvestmentStatistic> GetInvestmentsByCurrency(List<PortfolioDataRow> portfolioData, IList<ITradingItem> tradingItems, List<MonetaryDataRow> monetaryData)
        {
            var stockTrades = tradingItems.OfType<Trade>()
                                    .Where(t => t.IsShareTrade);

            var stockDividendGains = tradingItems
                .OfType<Dividend>()
                .GroupBy(t => t.Symbol, (symbol, symbolDivs) => (symbol, symbolDivs.Sum(sd => sd.Paied)))
                .ToDictionary(k=> k.symbol, v=> v.Item2);

            var symbolPrices = portfolioData
                .ToDictionary(
                    k => k.Symbol,
                    v => new PortfolioSymbolStatistic
                    {
                        Name = v.Name,
                        Price = v.UnitPrice ?? (decimal)0.0,
                        Amount = v.Amount
                    });

            var tradingFees = tradingItems.OfType<Trade>().Select(t => (t.Currency, t.Fee));
            var otherFees = tradingItems.OfType<Fee>().Select(t => (t.Currency, t.Paied));

            var feesPerCurrency = tradingFees
               .Concat(otherFees)
               .GroupBy(f => f.Currency, (c, fs) => (c, fs.Sum(ff => ff.Item2)))
               .ToDictionary(k => k.c, v => v.Item2);

            var moneyByCurrency = monetaryData.ToDictionary(k=> k.Currency);

            var currencyBalanceSheets = stockTrades
                .GroupBy(
                    t => (t.Symbol, t.Currency),
                    (cs, ts) => ComputeSymbolBalance(
                        cs,
                        ts,
                        symbolPrices.GetValueOrDefault(cs.Symbol)
                            ?? new PortfolioSymbolStatistic
                            {
                                Name = cs.Symbol,
                                Amount = 0,
                                Price = 0
                            },
                        stockDividendGains.GetValueOrDefault(cs.Symbol)))
                .GroupBy(sb => sb.Currency, (c, symbols) => new CurrencyInvestmentStatistic
                {
                    Currency = c,
                    Symbols = symbols.OrderByDescending(s => s.InvestedNowPrice ).ToList(),
                    Fees = feesPerCurrency[c],
                    Cash = moneyByCurrency.GetValueOrDefault(c)?.BuyingPower ?? (decimal)0.0
                })
                .ToList();
            return currencyBalanceSheets;
        }

        public TotalInvestmentStatistic ComputeTotalInvestment(CurrencyInvestmentStatistic statistic)
        {
            return new TotalInvestmentStatistic
            {
                InvestedNowPrice = statistic.Symbols.Sum(sb => sb.InvestedNowPrice),
                InvestedNow = statistic.Symbols.Sum(sb => sb.InvestedNow),
                InvestedAllTime = statistic.Symbols.Sum(sb => sb.InvestedAllTime) + statistic.Fees,
                RealizedGains = statistic.Symbols.Sum(sb => sb.RealizedGains)
            };
        }

        public InvestmentBalanceStatistic ComputeInvestmentBalance(CurrencyInvestmentStatistic statistic)
        {
            var total = statistic.Symbols.Sum(s => s.InvestedNowPrice);
            var symbolRisks = GetRiskForSymbols();
            var riskColors = GetRiskColors();

            return new InvestmentBalanceStatistic
            {
                Symbols = statistic.Symbols.Select(s => new SymbolBalance
                {
                    Name = s.Name,
                    Percentage = s.InvestedNowPrice / total * 100,
                    Risk = symbolRisks[s.Name],
                    RiskColor = riskColors[symbolRisks[s.Name]]
                })
                .OrderBy(s=> s.Risk)
                .ToList(),
                TotalInvestment = total
            };
        }

        private SymbolInvestmentStatistic ComputeSymbolBalance(
            (string Symbol, string Currency) currencySymbol,
            IEnumerable<Trade> symbolTrades,
            PortfolioSymbolStatistic portfolioSymbolStats,
            decimal dividendGains)
        {
            var alllTrades = symbolTrades.OrderByDescending(s => s.Date).ToList();

            var tradeStack = new Stack<Trade>(alllTrades);
            var currentBuyStack = new Stack<Trade>();

            var netSellGains = 0.0m;
            while (tradeStack.Count != 0)
            {
                var trade = tradeStack.Pop();

                if (trade.IsAcquire)
                {
                    currentBuyStack.Push(trade);
                    continue;
                }

                var unsatisfiedAmount = trade.Amount;
                while (unsatisfiedAmount > 0)
                {
                    if(unsatisfiedAmount > 0 && currentBuyStack.Count == 0)
                    {
                        throw new InvalidOperationException("At one point there is not enough shares to satisfy a buy. This is a api error.");
                    }

                    var nextBuy = currentBuyStack.Pop();
                    var amountSatisfied =
                        unsatisfiedAmount >= nextBuy.Amount // is remaining sell amount fully satisfied by this buy?
                        ? nextBuy.Amount
                        : unsatisfiedAmount;

                    netSellGains += amountSatisfied * (trade.UnitPrice-nextBuy.UnitPrice);

                    unsatisfiedAmount -= amountSatisfied;

                    if (nextBuy.Amount > amountSatisfied)
                    {
                        //we are not done yet with this buy, some shares are still unsold
                        currentBuyStack.Push(nextBuy.Copy(nextBuy.Amount - amountSatisfied));
                    }
                }
            }

            var netInvested = currentBuyStack.Sum(b => b.UnitPrice * b.Amount);
            var netAmount = currentBuyStack.Sum(b => b.Amount);

            if (netAmount != decimal.Zero && netAmount != portfolioSymbolStats.Amount)
            {
                throw new InvalidOperationException("Net amount of shares computed from trades does not match amount in portfolio. This is a api error.");
            }

            return new SymbolInvestmentStatistic
            {
                Name = currencySymbol.Symbol,
                Currency = currencySymbol.Currency,
                RealizedGains = netSellGains + dividendGains,
                InvestedNow = netInvested,
                InvestedNowPrice = netAmount * portfolioSymbolStats.Price,
                InvestedAllTime = alllTrades.Where(t=> t.IsAcquire).Sum(s => s.UnitPrice * s.Amount),
                BasePrice = netAmount != decimal.Zero
                    ? netInvested / netAmount
                    : decimal.Zero
            };
        }

        public IDictionary<string, Risk> GetRiskForSymbols()
        {
            return new Dictionary<string, Risk> {
                { "GOOGL", Risk.Low},
                { "TSLA", Risk.Moderate },
                { "MSFT", Risk.Low },
                { "CAT", Risk.Low },
                { "IRDM", Risk.High },
                { "FUV", Risk.High },
                { "DBX", Risk.Moderate },
                { "CSCO", Risk.Low },
                { "BFAHDIVI", Risk.Dead },
                { "BFAAGFIE", Risk.Dead },
                { "BAATELEC", Risk.Low },
                { "BAACHIRP", Risk.Dead },
                { "BAACEZ", Risk.Low }
            };
        }

        public IDictionary<Risk, string> GetRiskColors()
        {
            return new Dictionary<Risk, string> {
                { Risk.High, "#f00"},
                { Risk.Moderate, "#fa0" },
                { Risk.Low, "#2f0" },
                { Risk.Dead, "#000"}
            };
        }
    }
}
