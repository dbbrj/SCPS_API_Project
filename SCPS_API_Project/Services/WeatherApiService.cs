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
                return CreateMockWeatherData("API Key missing");
            }

            // Multiple endpoints to try, in order of preference
            // Based on Weather Company API: https://docs.weather.com/docs/read/Current_Observations_API
            var endpoints = new[]
            {
                // Standard v1 observations endpoint (works for most plans)
                new { Url = $"https://api.weather.com/v1/observations?geocode={lat},{lon}&apiKey={apiKey}&language=en-US", Format = "v1" },
                // v2 observations with coordinates (newer format)
                new { Url = $"https://api.weather.com/v2/observations?coordinates={lat},{lon}&apiKey={apiKey}&units=m", Format = "v2" },
                // v3 wx observations
                new { Url = $"https://api.weather.com/v3/wx/observations/current?geocode={lat},{lon}&apiKey={apiKey}", Format = "v3" }
            };

            foreach (var endpoint in endpoints)
            {
                var result = await TryFetchWeatherFromUrlAsync(endpoint.Url, endpoint.Format);
                if (result != null)
                {
                    _logger.LogInformation("Successfully fetched weather using {Format} endpoint", endpoint.Format);
                    return result;
                }
            }

            _logger.LogError("All weather API endpoints failed. Your API key may not have the required permissions for observations endpoints. Returning mock data.");
            // Return mock data so the app still works while we figure out the right endpoint
            return CreateMockWeatherData("All endpoints failed");
        }

        private WeatherModel CreateMockWeatherData(string reason)
        {
            _logger.LogWarning("Using mock weather data - {Reason}", reason);
            return new WeatherModel
            {
                TimeStamp = DateTime.UtcNow,
                Temperature = 15.5,
                WindSpeed = 12,
                WindDirection = "NW",
                SkyCondition = "Partly Cloudy",
                WxPhrase = "Partly cloudy",
                Location = "Odense, Denmark (MOCK)"
            };
        }

        private async Task<WeatherModel?> TryFetchWeatherFromUrlAsync(string url, string format = "v1")
        {
            try
            {
                _logger.LogDebug("Trying {Format} endpoint: {Url}", format, url.Split("?")[0]);

                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogDebug("Endpoint {Format} returned {StatusCode}: {Message}", 
                        format, response.StatusCode, errorContent.Length > 100 ? errorContent.Substring(0, 100) : errorContent);
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("API Response received from {Format}: {Length} bytes", format, json.Length);

                // Try to parse based on format
                if (format == "v3")
                {
                    var v3Response = JsonSerializer.Deserialize<WeatherApiResponseV3>(json, _jsonOptions);
                    if (v3Response?.Observations?.Count > 0)
                    {
                        var obs = v3Response.Observations.FirstOrDefault();
                        if (obs != null)
                        {
                            _logger.LogInformation("Successfully parsed v3 response: {Temp}°C", obs.Temperature);
                            return new WeatherModel
                            {
                                TimeStamp = DateTime.UtcNow,
                                Temperature = obs.Temperature ?? 0,
                                WindSpeed = obs.WindSpeed ?? 0,
                                WindDirection = obs.WindDirectionCardinal ?? "N/A",
                                SkyCondition = obs.CloudCoverPhrase ?? "Unknown",
                                WxPhrase = obs.WxPhrase,
                                Location = "Odense, Denmark"
                            };
                        }
                    }
                }
                else // v1 or v2
                {
                    var v1Response = JsonSerializer.Deserialize<WeatherApiResponse>(json, _jsonOptions);
                    if (v1Response?.Observations?.Count > 0)
                    {
                        var obs = v1Response.Observations.FirstOrDefault();
                        if (obs != null)
                        {
                            _logger.LogInformation("Successfully parsed {Format} response: {Temp}°C", format, obs.Temp);
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
                    }
                }

                _logger.LogDebug("Response parsed but no observations found in {Format} format", format);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Exception trying {Format} endpoint", format);
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
