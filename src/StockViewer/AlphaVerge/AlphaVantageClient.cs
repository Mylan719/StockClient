using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace StockViewer.AlphaVerge
{

    public class AlphaVantageClient : IDisposable
    {
        private readonly string apiKey;
        private readonly HttpClient httpClient;

        public AlphaVantageClient(string apiKey)
        {
            this.apiKey = apiKey;
            httpClient = new HttpClient();
        }

        public async Task<GlobalQuoteData> GetQuoteDataAsync(string symbol)
        {
            var parameters = new Dictionary<string, string>
            {
                {"function", "GLOBAL_QUOTE"},
                {"symbol", symbol},
                {"apikey", apiKey}
            };

            return (await GetDataInternalAsync<GlobalQuoteDataRoot>(parameters))?.Data;
        }

        public async Task<CurrencyData> GetCurrencyDataAsync(string fromSymbol, string toSymbol)
        {
            var parameters = new Dictionary<string, string>
            {
                {"function", "CURRENCY_EXCHANGE_RATE"},
                {"from_currency", fromSymbol},
                {"to_currency", toSymbol},
                {"apikey", apiKey}
            };

            return (await GetDataInternalAsync<CurrencyDataRoot>(parameters))?.Data;
        }

        private async Task<T> GetDataInternalAsync<T>(Dictionary<string, string> parameters)
            where T : class, new()
        {
            var stringRequest = parameters.Aggregate(@"https://www.alphavantage.co/query?", (current, param) => current + $"&{param.Key}={param.Value}");

            try
            {
                var result = await httpClient.GetStringAsync(stringRequest);
                return JsonConvert.DeserializeObject<T>(result);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void Dispose()
        {
            httpClient.Dispose();
        }

        private class CurrencyDataRoot
        {
            [JsonProperty("Realtime Currency Exchange Rate")]
            public CurrencyData Data { get; set; }
        }

        private class GlobalQuoteDataRoot
        {
            [JsonProperty("Global Quote")]
            public GlobalQuoteData Data { get; set; }
        }
    }
}