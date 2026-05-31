using System.Text.Json;
using SCPS_API_Project.Models;

namespace SCPS_API_Project.Services
{
    public interface IWeatherApiService
    {
        Task<WeatherModel?> FetchCurrentWeatherAsync();
    }

    // Facade: wraps the TWC API behind a simple interface
    public class WeatherApiService : IWeatherApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly ILogger<WeatherApiService> _logger;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public WeatherApiService(HttpClient httpClient, IConfiguration config, ILogger<WeatherApiService> logger)
        {
            _httpClient = httpClient;
            _config = config;
            _logger = logger;
        }

        /// <summary>
        /// Fetches current conditions from the TWC Currents on Demand endpoint.
        /// https://developer.weather.com/docs/openapi/currents-on-demand-3-0/get-wx-observations-current-by-geocode
        /// </summary>
        public async Task<WeatherModel?> FetchCurrentWeatherAsync()
        {
            var apiKey = _config["WeatherApi:ApiKey"] ?? string.Empty;
            var lat    = _config["WeatherApi:Latitude"]  ?? "55.6722880";
            var lon    = _config["WeatherApi:Longitude"] ?? "11.4797220";

            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogError("WeatherApi:ApiKey is not configured.");
                return null;
            }

            try
            {
                var url = $"https://api.weather.com/v3/wx/observations/current" +
                          $"?geocode={lat},{lon}&units=m&language=en-US&format=json&apiKey={apiKey}";

                var response = await _httpClient.GetAsync(url);
                var json     = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("CoD raw (first 600): {Json}", json.Length > 600 ? json[..600] : json);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("CoD endpoint returned {StatusCode}", response.StatusCode);
                    return null;
                }

                var cod = JsonSerializer.Deserialize<WeatherApiResponseV3CoD>(json, _jsonOptions);

                if (cod?.Temperature.HasValue == true)
                {
                    _logger.LogInformation("Fetched current weather: {Temp}°C", cod.Temperature);
                    return new WeatherModel
                    {
                        TimeStamp     = DateTime.UtcNow,
                        Temperature   = cod.Temperature.Value,
                        WindSpeed     = cod.WindSpeed ?? 0,
                        WindDirection = cod.WindDirectionCardinal ?? "N/A",
                        SkyCondition  = cod.CloudCover.HasValue ? $"{cod.CloudCover}% cloud cover" : "Unknown",
                        WxPhrase      = cod.WxPhraseLong ?? cod.WxPhraseShort ?? "Unknown",
                        Location      = "Stigs Bjergby, Denmark"
                    };
                }

                _logger.LogWarning("CoD response parsed but temperature field is absent");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch current weather");
                return null;
            }
        }
    }
}
