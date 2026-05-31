using System.Text.Json.Serialization;

namespace SCPS_API_Project.Models
{
    // v1/v2 API Response Structure (legacy)
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

    // v3 Currents On Demand (CoD) Response Structure
    // https://developer.weather.com/docs/openapi/currents-on-demand-3-0
    public class WeatherApiResponseV3CoD
    {
        [JsonPropertyName("temperature")]
        public double? Temperature { get; set; }

        [JsonPropertyName("temperatureFeelsLike")]
        public double? TemperatureFeelsLike { get; set; }

        [JsonPropertyName("temperatureDewPoint")]
        public double? TemperatureDewPoint { get; set; }

        [JsonPropertyName("windSpeed")]
        public double? WindSpeed { get; set; }

        [JsonPropertyName("windDirection")]
        public int? WindDirection { get; set; }

        [JsonPropertyName("windDirectionCardinal")]
        public string? WindDirectionCardinal { get; set; }

        [JsonPropertyName("windGust")]
        public double? WindGust { get; set; }

        [JsonPropertyName("pressureMeanSeaLevel")]
        public double? PressureMeanSeaLevel { get; set; }

        [JsonPropertyName("relativeHumidity")]
        public int? RelativeHumidity { get; set; }

        [JsonPropertyName("visibility")]
        public double? Visibility { get; set; }

        [JsonPropertyName("cloudCover")]
        public int? CloudCover { get; set; }

        [JsonPropertyName("cloudCeiling")]
        public int? CloudCeiling { get; set; }

        [JsonPropertyName("wxPhraseLong")]
        public string? WxPhraseLong { get; set; }

        [JsonPropertyName("wxPhraseShort")]
        public string? WxPhraseShort { get; set; }

        [JsonPropertyName("iconCode")]
        public int? IconCode { get; set; }

        [JsonPropertyName("iconCodeExtend")]
        public int? IconCodeExtend { get; set; }

        [JsonPropertyName("dayOfWeek")]
        public string? DayOfWeek { get; set; }

        [JsonPropertyName("dayOrNight")]
        public string? DayOrNight { get; set; }

        [JsonPropertyName("validTimeUtc")]
        public long? ValidTimeUtc { get; set; }

        [JsonPropertyName("validTimeLocal")]
        public string? ValidTimeLocal { get; set; }

        [JsonPropertyName("uvIndex")]
        public int? UvIndex { get; set; }

        [JsonPropertyName("uvDescription")]
        public string? UvDescription { get; set; }

        [JsonPropertyName("precip1Hour")]
        public double? Precip1Hour { get; set; }

        [JsonPropertyName("precip6Hour")]
        public double? Precip6Hour { get; set; }

        [JsonPropertyName("precip24Hour")]
        public double? Precip24Hour { get; set; }
    }

    // v3 Hourly Forecast Response Structure
    public class WeatherApiResponseV3Hourly
    {
        [JsonPropertyName("temperature")]
        public List<double> Temperature { get; set; } = new();

        [JsonPropertyName("temperatureFeelsLike")]
        public List<double> TemperatureFeelsLike { get; set; } = new();

        [JsonPropertyName("windSpeed")]
        public List<double> WindSpeed { get; set; } = new();

        [JsonPropertyName("windDirection")]
        public List<int> WindDirection { get; set; } = new();

        [JsonPropertyName("windDirectionCardinal")]
        public List<string> WindDirectionCardinal { get; set; } = new();

        [JsonPropertyName("wxPhraseLong")]
        public List<string> WxPhraseLong { get; set; } = new();

        [JsonPropertyName("cloudCover")]
        public List<int> CloudCover { get; set; } = new();

        [JsonPropertyName("relativeHumidity")]
        public List<int> RelativeHumidity { get; set; } = new();

        [JsonPropertyName("validTimeLocal")]
        public List<string> ValidTimeLocal { get; set; } = new();
    }

    // v3 Observations Response (array format)
    public class WeatherApiResponseV3Observations
    {
        [JsonPropertyName("observations")]
        public List<ObservationV3CoD> Observations { get; set; } = new();
    }

    public class ObservationV3CoD
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

        [JsonPropertyName("wxPhraseLong")]
        public string? WxPhraseLong { get; set; }

        [JsonPropertyName("observationTime")]
        public string? ObservationTime { get; set; }
    }

    // Legacy v3 Observations Response (kept for backward compatibility)
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

    // v3 Historical Observations — parallel arrays, one entry per hourly slot
    public class WeatherApiResponseV3Historical
    {
        [JsonPropertyName("temperature")]
        public List<int?> Temperature { get; set; } = new();

        [JsonPropertyName("windSpeed")]
        public List<int?> WindSpeed { get; set; } = new();

        [JsonPropertyName("windDirectionCardinal")]
        public List<string?> WindDirectionCardinal { get; set; } = new();

        [JsonPropertyName("wxPhraseLong")]
        public List<string?> WxPhraseLong { get; set; } = new();

        [JsonPropertyName("validTimeLocal")]
        public List<string?> ValidTimeLocal { get; set; } = new();

        [JsonPropertyName("validTimeUtc")]
        public List<long?> ValidTimeUtc { get; set; } = new();

        [JsonPropertyName("relativeHumidity")]
        public List<int?> RelativeHumidity { get; set; } = new();

        [JsonPropertyName("cloudCeiling")]
        public List<int?> CloudCeiling { get; set; } = new();

        [JsonPropertyName("windGust")]
        public List<int?> WindGust { get; set; } = new();

        [JsonPropertyName("pressureMeanSeaLevel")]
        public List<double?> PressureMeanSeaLevel { get; set; } = new();

        [JsonPropertyName("uvIndex")]
        public List<int?> UvIndex { get; set; } = new();
    }

    // v3 15-Minute Forecast — parallel arrays, one entry per 15-minute interval
    // https://developer.weather.com/docs/openapi/15-minute-forecast-3-0/get-wx-forecast-fifteenminute
    public class WeatherApiResponseFifteenMinuteForecast
    {
        [JsonPropertyName("temperature")]
        public List<int?> Temperature { get; set; } = new();

        [JsonPropertyName("windSpeed")]
        public List<int?> WindSpeed { get; set; } = new();

        [JsonPropertyName("windDirectionCardinal")]
        public List<string?> WindDirectionCardinal { get; set; } = new();

        [JsonPropertyName("wxPhraseLong")]
        public List<string?> WxPhraseLong { get; set; } = new();

        [JsonPropertyName("validTimeLocal")]
        public List<string?> ValidTimeLocal { get; set; } = new();

        [JsonPropertyName("validTimeUtc")]
        public List<long?> ValidTimeUtc { get; set; } = new();

        [JsonPropertyName("relativeHumidity")]
        public List<int?> RelativeHumidity { get; set; } = new();

        [JsonPropertyName("cloudCover")]
        public List<int?> CloudCover { get; set; } = new();

        [JsonPropertyName("windGust")]
        public List<int?> WindGust { get; set; } = new();

        [JsonPropertyName("precipChance")]
        public List<int?> PrecipChance { get; set; } = new();

        [JsonPropertyName("precipType")]
        public List<string?> PrecipType { get; set; } = new();
    }
}
