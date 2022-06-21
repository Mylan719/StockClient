using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;
using Microsoft.Extensions.Options;
using StockViewer.BL;
using StockViewer.Fio.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace StockViewer.Fio
{
    public partial class FioClient : IDisposable
    {
        private readonly HttpClient httpClient = new HttpClient();
        private readonly string baseFioUrl;
        private string[] loginCookies;

        public FioClient(IOptions<FioOptions> fioOptions)
        {
            this.baseFioUrl = fioOptions.Value?.BaseUrl ?? throw new InvalidOperationException("Configure Fio.BaseUrl in appsettings.");
        }

        public async Task LoginAsync(string userName, string password)
        {
            var inputs = await GetLoginFormDataAsync();

            inputs["LOGIN_USERNAME"] = userName;
            inputs["LOGIN_PASSWORD"] = password;

            var requestPayload = new FormUrlEncodedContent(inputs.AsEnumerable().ToArray());

            httpClient.DefaultRequestHeaders.Add("Host", "ebroker.fio.cz");
            httpClient.DefaultRequestHeaders.Add("Origin", baseFioUrl);
            httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "document");
            httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "navigate");
            httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Site", "same-origin");
            httpClient.DefaultRequestHeaders.Add("Sec-Fetch-User", "?1");
            httpClient.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
            httpClient.DefaultRequestHeaders.Add("User-Agent", "StockViewer.Client v1");

            var response = await httpClient.PostAsync($"{baseFioUrl}/e-broker.cgi", requestPayload);

            loginCookies = response.Headers.Where(h => h.Key == "Set-Cookie").SelectMany(c => c.Value).ToArray();
        }

        public async Task<HtmlDocument> GetHtmlDocumentAsync(string relativeUrl)
        {
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"{baseFioUrl}/{relativeUrl}"))
            {
                requestMessage.Headers.Add("Cookie", string.Join("; ", loginCookies));

                var response = await httpClient.SendAsync(requestMessage);

                return await GetHtmlDocumentAsync(response);
            }
        }

        public async Task<List<TradeDataRow>> GetTradeDataAsync(DateTime from, DateTime to)
        {
            var fromString = from.ToString("dd.MM.yyyy");
            var toString = to.ToString("dd.MM.yyyy");

            var document = await GetHtmlDocumentAsync($"e-obchody.cgi?obchody_DAT_od={fromString}&obchody_DAT_do={toString}");

            var tradesTable = document.DocumentNode.QuerySelector("table#obchody_full_table")
                ?? throw new InvalidOperationException("Could not find expected data table in the html document. Are you logged in?");

            var tradesMatrix = CreateMatrixFromHtmlTable(tradesTable);

            return tradesMatrix
            .Skip(2) //skip header and footer
            .SkipLast(1)
            .Select(r => new TradeDataRow
            {
                Date = DateTime.Parse(r[0], CultureInfo.CreateSpecificCulture("cs-CZ")),
                Type = r[1],
                Symbol = r[2],
                Price = ToDecimal(r[3]),
                Amount = ToDecimal(r[4]),
                Currency = r[5],
                ValueCzk = ToDecimal(r[6]),
                FeesCzk = ToDecimal(r[7]),
                ValueUsd = ToDecimal(r[8]),
                FeesUsd = ToDecimal(r[9]),
                ValueEur = ToDecimal(r[10]),
                FeesEur = ToDecimal(r[11]),
                Description = r[12]
            })
            .ToList();
        }

        public async Task<List<PortfolioDataRow>> GetPortfolioDataAsync()
        {
            var document = await GetHtmlDocumentAsync("e-portfolio.cgi?menu=0");

            var portfolio = document.DocumentNode.QuerySelector("table#portfolio_table")
            ?? throw new InvalidOperationException("Could not find expected data table in the html document. Are you logged in?");

            var portfolioMatrix = CreateMatrixFromHtmlTable(portfolio);

            var portfolioData = portfolioMatrix
                .Skip(2)
                .SkipLast(1)
                .Select(r =>
                {
                    return new PortfolioDataRow
                    {
                        Name = r[1],
                        Symbol = r[2],
                        Amount = ToDecimal(r[3]) ?? throw new ArgumentException("Amount should be always set for portfolio data."),
                        UnitPrice = ToDecimal(r[4]),
                        CloseChangePercentage = ToDecimal(r[5]),
                        SharesTraded = ToDecimal(r[6]),
                        ValueByFio = ToDecimal(r[7]),
                        CloseChangeByFio = ToDecimal(r[8]),
                        AmountBlockedInCdcp = ToDecimal(r[9]),
                    };
                })
            .ToList();
            return portfolioData;
        }

        public async Task<List<MonetaryDataRow>> GetMonetaryDataAsync()
        {
            var document = await GetHtmlDocumentAsync("e-penize.cgi");

            var moneyTable = document.DocumentNode.QuerySelector("table#penize_table")
                ?? throw new InvalidOperationException("Could not find expected data table in the html document. Are you logged in?");

            var moneyMatrix = CreateMatrixFromHtmlTable(moneyTable);

            return moneyMatrix
            .Skip(2) //skip header and footer
            .Select(r => new MonetaryDataRow
            {
                Currency = r[2],
                Total = ToDecimal(r[3]),
                InTransfer = ToDecimal(r[4]),
                InBuys = ToDecimal(r[5]),
                InSales = ToDecimal(r[6]),
                BuyingPower = ToDecimal(r[7]) ?? (decimal)0.0,
                ToWithdraw = ToDecimal(r[8]) ?? (decimal)0.0,
            })
            .ToList();
        }

        private async Task<Dictionary<string, string>> GetLoginFormDataAsync()
        {
            var response = await httpClient.GetAsync($"{baseFioUrl}e-broker.cgi");
            var document = await GetHtmlDocumentAsync(response);

            var inputs = document.DocumentNode
                .QuerySelectorAll("form[name=LF] input")
                .Select(i => new FormData
                {
                    Name = i.GetAttributeValue("name", null),
                    Value = i.GetAttributeValue("value", ""),
                    InputType = i.GetAttributeValue("type", "")
                })
                .Where(d => !string.IsNullOrWhiteSpace(d.Name))
                .Where(d => !string.Equals(d.InputType, "submit", StringComparison.OrdinalIgnoreCase))
                .ToDictionary(k => k.Name, v => v.Value);
            return inputs;
        }

        private static async Task<HtmlDocument> GetHtmlDocumentAsync(HttpResponseMessage response)
        {
            var stream = await response.Content.ReadAsStreamAsync();
            var document = new HtmlDocument();
            document.Load(stream);
            return document;
        }

        private static List<List<string>> CreateMatrixFromHtmlTable(HtmlNode portfolio)
        {
            return portfolio
                .QuerySelectorAll("tr")
                .Select(row =>
                {
                    return row
                        .QuerySelectorAll("th, td")
                        .SelectMany(t => GetNextCellSpan(t))
                        .ToList();
                })
                .ToList();
        }

        private static decimal? ToDecimal(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return null;
            }

            try
            {
                return decimal.Parse(
                    input.Replace("%", ""),
                    CultureInfo.CreateSpecificCulture("cs-CZ"));
            }
            catch (FormatException)
            {
                throw new ArgumentException($"Input <{input}> could not be parsed to decimal.");
            }
        }

        //this is to take care of rowspans so the matrix has equal row lenghts
        private static IEnumerable<string> GetNextCellSpan(HtmlNode t)
        {
            var spanlenght = t.GetAttributeValue("colspan", 1);

            var imgTitles = t.Descendants().Where(n => string.Equals("img", n.Name, StringComparison.OrdinalIgnoreCase))
                .Select(n => n.GetAttributeValue("title", null))
                .ToList();

            if (imgTitles.Any())
            {
                yield return string.Join(" ", imgTitles);
            }
            else
            {
                yield return t.InnerText
                    .Replace("&nbsp;", "")
                    .Trim();
            }

            for (int i = 1; i < spanlenght; i++)
            {
                yield return string.Empty;
            }
        }

        public void Dispose()
        {
            httpClient.Dispose();
        }
    }
}
