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
        public List<CurrencyInvestmentStatistic> GetInvestmentsByCurrency(List<PortfolioDataRow> portfolioData, IList<ITradingItem> tradingItems)
        {
            var stockTrades = tradingItems.OfType<Trade>()
                                    .Where(t => t.IsShareTrade);

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

            var currencyBalanceSheets = stockTrades
                .GroupBy(t => (t.Symbol, t.Currency), (cs, ts) => ComputeSymbolBalance(cs, ts, symbolPrices[cs.Symbol]))
                .GroupBy(sb => sb.Currency, (c, symbols) => new CurrencyInvestmentStatistic
                {
                    Currency = c,
                    Symbols = symbols.ToList(),
                    Fees = feesPerCurrency[c],
                })
                .ToList();
            return currencyBalanceSheets;
        }

        public TotalInvestmentStatistic ComputeTotalInvestment(CurrencyInvestmentStatistic statistic)
        {
            return new TotalInvestmentStatistic
            {
                AllTimeInvestmentValue = statistic.Symbols.Sum(sb => sb.InvestedNowPrice) + statistic.Symbols.Sum(s => s.RealizedGains),
                AllTimeInvested = statistic.Symbols.Sum(sb => sb.InvestedAllTime) + statistic.Fees
            };
        }

        public InvestmentBalanceStatistic ComputeInvestmentBalance(CurrencyInvestmentStatistic statistic)
        {
            var total = statistic.Symbols.Sum(s => s.InvestedNow);

            return new InvestmentBalanceStatistic
            {
                SymbolPercentages = statistic.Symbols.ToDictionary(k=> k.Name, v=> v.InvestedNow / total*100),
                TotalInvestment = total
            };
        }

        private SymbolInvestmentStatistic ComputeSymbolBalance(
            (string Symbol, string Currency) currencySymbol,
            IEnumerable<Trade> symbolTrades,
            PortfolioSymbolStatistic portfolioSymbolStats)
        {
            var symbolsIn = symbolTrades.OrderBy(s => s.Date).Where(t => t.IsAcquire).ToList();
            var symbolsOut = symbolTrades.OrderBy(s => s.Date).Where(t => t.IsDispose).ToList();

            var buyStack = new Stack<Trade>(symbolsIn);

            var netSellGains = (decimal)0;
            foreach (var sell in symbolsOut)
            {
                var unsatisfiedAmount = sell.Amount;
                while (unsatisfiedAmount > 0)
                {
                    var nextBuy = buyStack.Pop();
                    var amountSatisfied =
                        unsatisfiedAmount >= nextBuy.Amount // is remaining sell amount fully satisfied by this buy?
                        ? nextBuy.Amount
                        : unsatisfiedAmount;

                    netSellGains += amountSatisfied * nextBuy.UnitPrice;

                    unsatisfiedAmount -= amountSatisfied;

                    if (nextBuy.Amount > amountSatisfied)
                    {
                        //we are not done yet with this buy, some shares are still unsold
                        buyStack.Push(nextBuy.Copy(nextBuy.Amount - amountSatisfied));
                    }
                }
            }

            var netInvested = buyStack.Sum(b => b.UnitPrice * b.Amount);
            var netAmount = buyStack.Sum(b => b.Amount);

            if (netAmount != decimal.Zero && netAmount != portfolioSymbolStats.Amount)
            {
                throw new InvalidOperationException("Net amount of shares computed from trades does not match amount in portfolio. This is a api error.");
            }

            return new SymbolInvestmentStatistic
            {
                Name = currencySymbol.Symbol,
                Currency = currencySymbol.Currency,
                RealizedGains = netSellGains,
                InvestedNow = netInvested,
                InvestedNowPrice = netAmount * portfolioSymbolStats.Price,
                InvestedAllTime = symbolsIn.Sum(s => s.UnitPrice * s.Amount)
            };
        }
    }
}
