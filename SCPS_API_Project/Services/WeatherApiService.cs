using System.Text.Json;
using SCPS_API_Project.Models;

namespace SCPS_API_Project.Services
{
    public interface IWeatherApiService
    {
        Task<WeatherModel?> FetchCurrentWeatherAsync();
    }

    // Facade: hides the complexity of the external weather.com API behind a simple interface
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

        public async Task<WeatherModel?> FetchCurrentWeatherAsync()
        {
            var apiKey = _config["WeatherApi:ApiKey"];
            var lat = _config["WeatherApi:Latitude"] ?? "55.6761";
            var lon = _config["WeatherApi:Longitude"] ?? "12.5683";

            // Check for missing API key early
            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogError("WeatherApi:ApiKey is not configured. Please set it in appsettings.json or user secrets.");
                return null;
            }

            // Using v2/observations - standard endpoint available on most plans
            // Format: /v2/observations?coordinates=latitude,longitude&apiKey=KEY
            var url = $"https://api.weather.com/v2/observations?coordinates={lat},{lon}&apiKey={apiKey}&language=en-US";

            try
            {
                _logger.LogInformation("API Key length: {KeyLength} characters", apiKey.Length);
                _logger.LogDebug("Requesting weather data from: {Url}", url.Replace(apiKey, "***REDACTED***"));

                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("API returned status {StatusCode}: {ReasonPhrase}. Response: {Response}", 
                        response.StatusCode, response.ReasonPhrase, errorContent);

                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        _logger.LogError("401 Unauthorized - Check if API key is valid and has permission for this endpoint");
                    }
                    if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        _logger.LogError("400 Bad Request - Invalid request format or missing required parameters");
                    }
                }

                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("API Response: {Json}", json);

                var apiResponse = JsonSerializer.Deserialize<WeatherApiResponse>(json, _jsonOptions);
                var obs = apiResponse?.Observations.FirstOrDefault();

                if (obs == null)
                {
                    _logger.LogWarning("API returned no observations.");
                    return null;
                }

                _logger.LogInformation("Successfully fetched weather: {Temp}°C at {Location}", obs.Temp, obs.ObsName);

                return new WeatherModel
                {
                    TimeStamp = DateTime.UtcNow,
                    Temperature = obs.Temp ?? 0,
                    WindSpeed = obs.Wspd ?? 0,
                    WindDirection = obs.WdirCardinal ?? "N/A",
                    SkyCondition = MapSkyCondition(obs.Clds),
                    WxPhrase = obs.WxPhrase,
                    Location = obs.ObsName ?? "Unknown"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch weather data from weather.com");
                return null;
            }
        }

        private static string MapSkyCondition(string? clds) => clds switch
        {
            "CLR" => "Clear",
            "FEW" => "Few Clouds",
            "SCT" => "Scattered Clouds",
            "BKN" => "Broken Clouds",
            "OVC" => "Overcast",
            _ => clds ?? "Unknown"
        };
    }
}
