using System.Text.Json.Serialization;

namespace SCPS_API_Project.Models
{
    // v1/v2 API Response Structure
    public class WeatherApiResponse
    {
        [JsonPropertyName("observations")]
        public List<Observation> Observations { get; set; } = new();
    }

    public class Observation
    {
        [JsonPropertyName("temp")]
        public double? Temp { get; set; }

        [JsonPropertyName("wspd")]
        public double? Wspd { get; set; }

        [JsonPropertyName("wdir")]
        public int? Wdir { get; set; }

        [JsonPropertyName("wdir_cardinal")]
        public string? WdirCardinal { get; set; }

        [JsonPropertyName("clds")]
        public string? Clds { get; set; }

        [JsonPropertyName("wx_phrase")]
        public string? WxPhrase { get; set; }

        [JsonPropertyName("obsName")]
        public string? ObsName { get; set; }

        [JsonPropertyName("obsTimeUtc")]
        public string? ObsTimeUtc { get; set; }
    }

    // v3 API Response Structure
    public class WeatherApiResponseV3
    {
        [JsonPropertyName("observations")]
        public List<ObservationV3> Observations { get; set; } = new();
    }

    public class ObservationV3
    {
        [JsonPropertyName("temperature")]
        public double? Temperature { get; set; }

        [JsonPropertyName("windSpeed")]
        public double? WindSpeed { get; set; }

        [JsonPropertyName("windDirection")]
        public int? WindDirection { get; set; }

        [JsonPropertyName("windDirectionCardinal")]
        public string? WindDirectionCardinal { get; set; }

        [JsonPropertyName("cloudCoverPhrase")]
        public string? CloudCoverPhrase { get; set; }

        [JsonPropertyName("wxPhrase")]
        public string? WxPhrase { get; set; }

        [JsonPropertyName("observationTime")]
        public string? ObservationTime { get; set; }
    }
}
