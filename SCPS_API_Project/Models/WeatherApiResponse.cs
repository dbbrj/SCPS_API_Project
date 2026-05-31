using System.Text.Json.Serialization;

namespace SCPS_API_Project.Models
{
    // TWC Currents on Demand (CoD) response — single object, not an array
    // https://developer.weather.com/docs/openapi/currents-on-demand-3-0
    public class WeatherApiResponseV3CoD
    {
        [JsonPropertyName("temperature")]
        public double? Temperature { get; set; }

        [JsonPropertyName("windSpeed")]
        public double? WindSpeed { get; set; }

        [JsonPropertyName("windDirectionCardinal")]
        public string? WindDirectionCardinal { get; set; }

        [JsonPropertyName("cloudCover")]
        public int? CloudCover { get; set; }

        [JsonPropertyName("wxPhraseLong")]
        public string? WxPhraseLong { get; set; }

        [JsonPropertyName("wxPhraseShort")]
        public string? WxPhraseShort { get; set; }
    }
}
