using StockViewer.AlphaVerge;
using StockViewer.Fio;
using StockViewer.Fio.Data;
using StockViewer.Fio.Trading;
using StockViewer.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StockViewer
{
    partial class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            LoadDataFromDashboardAsync().Wait();
        }

        private static async Task LoadDataFromDashboardAsync()
        {
            using (var alphaClient = new AlphaVantageClient(Credentials.AlphaVergeToken))
            {
                using (var fioClient = new FioClient())
                {
                    await fioClient.LoginAsync(Credentials.FioUserName, Credentials.FioUserPassword);


                    var tradingItemProvider = new TradingItemProvider(fioClient);
                    var tradingStatisticsProvider = new TradingStatisticsProvider();

                    //not to get banned
                    await Task.Delay(100);
                    var portfolioData = await fioClient.GetPortfolioDataAsync();

                    //not to get banned
                    await Task.Delay(100);
                    var tradingItems = await tradingItemProvider.GetAllItemsAsync();

                    Console.WriteLine("Trades:");
                    tradingItems.ToList().ForEach(Console.WriteLine);
                    Console.WriteLine();

                    var currencyBalanceSheets = tradingStatisticsProvider.GetInvestmentsByCurrency(portfolioData, tradingItems);


                    foreach (var sheet in currencyBalanceSheets)
                    {
                        Console.WriteLine($"Symbol trended ({sheet.Currency}):");
                        sheet.Symbols.ForEach(Console.WriteLine);
                        Console.WriteLine("-----");
                        Console.WriteLine($"{sheet.Fees} paid in fees");
                        Console.WriteLine("=====");
                        Console.WriteLine(tradingStatisticsProvider.ComputeTotalInvestment(sheet));
                        Console.WriteLine();

                        var balance = tradingStatisticsProvider.ComputeInvestmentBalance(sheet);

                        Console.WriteLine("Balance:");
                        foreach (var symbolPercentage in balance.Symbols.OrderByDescending(s=> s.Percentage))
                        {
                            Console.WriteLine($"{symbolPercentage.Name}: {symbolPercentage:N2}%");
                        }
                        Console.WriteLine();
                        Console.WriteLine();
                    }


                    var usdStocks = portfolioData
                        .Where(p => p.UnitPrice.HasValue)
                        .Where(p => !p.Symbol.StartsWith("B") && !p.Symbol.EndsWith("(OU)")).ToList();

                    var usdStocksSum = usdStocks.Sum(p => p.Amount * p.UnitPrice);

                    var exchangeData = await alphaClient.GetCurrencyDataAsync("USD", "CZK");

                    var fioSum = usdStocks.Sum(p => p.ValueByFio);
                    var realSum = exchangeData.Rate * usdStocksSum;

                    Console.WriteLine($"Fio value (CZK):\t{fioSum}");
                    Console.WriteLine($"Value (CZK):\t\t{realSum}");
                    Console.WriteLine($"Difference:\t\t{realSum - fioSum}");
                    Console.WriteLine($"Fio takes:\t\t{(realSum - fioSum) * 100 / realSum}%");

                }
            }
            Console.ReadKey();
        }

        private static async Task<IDictionary<string, decimal>> GetSymbolPriceAsync(IEnumerable<string> symbols, AlphaVantageClient client)
        {
            var dict = new Dictionary<string, decimal>();
            foreach (var symbol in symbols)
            {
                await Task.Delay(1000);
                dict.Add(symbol, (await client.GetQuoteDataAsync(symbol))?.Price ?? throw new ArgumentException($"No data for symbol: {symbol}"));
            }
            return dict;
        }
    }
}
