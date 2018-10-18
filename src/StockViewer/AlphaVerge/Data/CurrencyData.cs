using Newtonsoft.Json;

namespace StockViewer.AlphaVerge
{
    public class CurrencyData
    {
        [JsonProperty("1. From_Currency Code")]
        public string FromCode { get; set; }
        [JsonProperty("2. From_Currency Name")]
        public string FromName { get; set; }
        [JsonProperty("3. To_Currency Code")]
        public string ToCode { get; set; }
        [JsonProperty("4. To_Currency Name")]
        public string ToName { get; set; }
        [JsonProperty("5. Exchange Rate")]
        public decimal Rate { get; set; }
        [JsonProperty("6. Last Refreshed")]
        public string RefreshedDate { get; set; }
        [JsonProperty("7. Time Zone")]
        public string TimeZone { get; set; }
    }
}