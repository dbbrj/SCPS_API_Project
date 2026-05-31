using System.Text.Json;
using SCPS_API_Project.Models;

namespace SCPS_API_Project.Services
{
    public interface IWeatherApiService
    {
        Task<WeatherModel?> FetchCurrentWeatherAsync();
        Task<List<WeatherModel>> FetchHistoricalHourlyAsync(int daysBack = 1);
        Task<List<WeatherModel>> FetchHistoricalHourlyAsync(DateTime? specificDate = null);
        Task<List<ForecastSlot>> FetchFifteenMinuteForecastAsync();
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

            // Correct Weather Company API v3 endpoints based on Standard Weather Data Package documentation
            // https://developer.weather.com/docs/standard-weather-data-package
            var endpoints = new[]
            {
                // Try Currents On Demand without units (sometimes units param causes 400)
                new { 
                    Url = $"https://api.weather.com/v3/wx/observations/current?geocode={lat},{lon}&apiKey={apiKey}&language=en-US", 
                    Format = "currents-on-demand-v1" 
                },
                // Try with just geocode and key
                new { 
                    Url = $"https://api.weather.com/v3/wx/observations/current?geocode={lat},{lon}&apiKey={apiKey}", 
                    Format = "currents-on-demand-v2" 
                },
                // Try the "on-demand" format (sometimes the endpoint differs slightly)
                new { 
                    Url = $"https://api.weather.com/v3/wx/observations/onDemand?geocode={lat},{lon}&apiKey={apiKey}", 
                    Format = "observations-on-demand" 
                },
                // Try v1 format with just essentials
                new { 
                    Url = $"https://api.weather.com/v1/observations?geocode={lat},{lon}&apiKey={apiKey}", 
                    Format = "v1-observations" 
                },
                // Site-Based Observations (another v1 variant)
                new { 
                    Url = $"https://api.weather.com/v1/observations?search={lat},{lon}&apiKey={apiKey}", 
                    Format = "v1-search" 
                }
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

            _logger.LogError("All weather API endpoints failed. Your API key may not have the required permissions for observations endpoints. Using mock data.");
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
                _logger.LogDebug("Trying {Format} endpoint: {Endpoint}", format, url.Split("?")[0]);

                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogDebug("Endpoint {Format} returned {StatusCode}", 
                        format, response.StatusCode);
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("API Response received from {Format}: {Length} bytes", format, json.Length);

                // Try to parse based on format
                if (format == "currents-on-demand")
                {
                    // Currents On Demand returns a single object (not wrapped in array)
                    var codResponse = JsonSerializer.Deserialize<WeatherApiResponseV3CoD>(json, _jsonOptions);
                    if (codResponse?.Temperature.HasValue == true)
                    {
                        _logger.LogInformation("Successfully parsed Currents On Demand response: {Temp}°C", codResponse.Temperature);
                        return new WeatherModel
                        {
                            TimeStamp = DateTime.UtcNow,
                            Temperature = codResponse.Temperature ?? 0,
                            WindSpeed = codResponse.WindSpeed ?? 0,
                            WindDirection = codResponse.WindDirectionCardinal ?? "N/A",
                            SkyCondition = codResponse.CloudCover.HasValue ? $"{codResponse.CloudCover}% cloud cover" : "Unknown",
                            WxPhrase = codResponse.WxPhraseLong ?? codResponse.WxPhraseShort,
                            Location = "Odense, Denmark"
                        };
                    }
                }
                else if (format == "hourly-forecast")
                {
                    // Hourly Forecast returns arrays of values
                    var forecastResponse = JsonSerializer.Deserialize<WeatherApiResponseV3Hourly>(json, _jsonOptions);
                    if (forecastResponse?.Temperature?.Count > 0)
                    {
                        var temp = forecastResponse.Temperature[0];
                        var windSpeed = forecastResponse.WindSpeed?[0] ?? 0;
                        var windDir = forecastResponse.WindDirectionCardinal?[0] ?? "N/A";
                        var wxPhrase = forecastResponse.WxPhraseLong?[0] ?? "Unknown";

                        _logger.LogInformation("Successfully parsed Hourly Forecast response: {Temp}°C", temp);
                        return new WeatherModel
                        {
                            TimeStamp = DateTime.UtcNow,
                            Temperature = temp,
                            WindSpeed = windSpeed,
                            WindDirection = windDir,
                            SkyCondition = $"{forecastResponse.CloudCover?[0] ?? 0}% cloud cover",
                            WxPhrase = wxPhrase,
                            Location = "Odense, Denmark"
                        };
                    }
                }
                else if (format == "pws-observations")
                {
                    // PWS Observations - try to parse similar to v1 observations
                    var v1Response = JsonSerializer.Deserialize<WeatherApiResponse>(json, _jsonOptions);
                    if (v1Response?.Observations?.Count > 0)
                    {
                        var obs = v1Response.Observations.FirstOrDefault();
                        if (obs != null)
                        {
                            _logger.LogInformation("Successfully parsed PWS Observations response: {Temp}°C", obs.Temp);
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
                else // v1, v2, v3 legacy formats
                {
                    // Try v3 Observations array first
                    var v3ObsResponse = JsonSerializer.Deserialize<WeatherApiResponseV3Observations>(json, _jsonOptions);
                    if (v3ObsResponse?.Observations?.Count > 0)
                    {
                        var obs = v3ObsResponse.Observations.FirstOrDefault();
                        if (obs != null)
                        {
                            _logger.LogInformation("Successfully parsed v3 Observations response: {Temp}°C", obs.Temperature);
                            return new WeatherModel
                            {
                                TimeStamp = DateTime.UtcNow,
                                Temperature = obs.Temperature ?? 0,
                                WindSpeed = obs.WindSpeed ?? 0,
                                WindDirection = obs.WindDirectionCardinal ?? "N/A",
                                SkyCondition = obs.CloudCoverPhrase ?? "Unknown",
                                WxPhrase = obs.WxPhraseLong ?? obs.WxPhrase,
                                Location = "Odense, Denmark"
                            };
                        }
                    }

                    // Try v1/v2 format
                    var v1Response = JsonSerializer.Deserialize<WeatherApiResponse>(json, _jsonOptions);
                    if (v1Response?.Observations?.Count > 0)
                    {
                        var obs = v1Response.Observations.FirstOrDefault();
                        if (obs != null)
                        {
                            _logger.LogInformation("Successfully parsed v1/v2 response: {Temp}°C", obs.Temp);
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

        /// <summary>
        /// Fetches historical hourly weather data for the specified number of days back
        /// Uses the v3/wx/conditions/historical/hourly/1day endpoint
        /// https://developer.weather.com/docs/openapi/historical-conditions-hourly-3-0/get-v3-wx-conditions-historical-hourly-1day-by-geocode
        /// </summary>
        public async Task<List<WeatherModel>> FetchHistoricalHourlyAsync(int daysBack = 1)
        {
            var apiKey = _config["WeatherApi:ApiKey"];
            var lat = _config["WeatherApi:Latitude"] ?? "55.6761";
            var lon = _config["WeatherApi:Longitude"] ?? "12.5683";
            var results = new List<WeatherModel>();

            // Check for missing API key early
            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogError("WeatherApi:ApiKey is not configured. Cannot fetch historical data.");
                return results;
            }

            try
            {
                // Historical endpoint: /v3/wx/conditions/historical/hourly/1day
                // For multiple days, you need to call for each day
                for (int i = 0; i < daysBack; i++)
                {
                    var url = $"https://api.weather.com/v3/wx/conditions/historical/hourly/1day?geocode={lat},{lon}&apiKey={apiKey}&units=m&language=en-US&format=json";

                    _logger.LogDebug("Fetching historical hourly data (day {Day}): {Endpoint}", i, url.Split("?")[0]);

                    var response = await _httpClient.GetAsync(url);

                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogWarning("Historical endpoint returned {StatusCode}", response.StatusCode);
                        continue;
                    }

                    var json = await response.Content.ReadAsStringAsync();

                    // Parse the historical response (arrays format)
                    var historicalResponse = JsonSerializer.Deserialize<WeatherApiResponseV3Historical>(json, _jsonOptions);

                    if (historicalResponse?.Temperature?.Count > 0)
                    {
                        _logger.LogInformation("Fetched {Count} historical hourly records", historicalResponse.Temperature.Count);

                        // Convert array format to individual WeatherModel records
                        for (int idx = 0; idx < historicalResponse.Temperature.Count; idx++)
                        {
                            var timeStr = historicalResponse.ValidTimeLocal?[idx];
                            var temp = historicalResponse.Temperature?[idx] ?? 0;
                            var windSpeed = historicalResponse.WindSpeed?[idx] ?? 0;
                            var windDir = historicalResponse.WindDirectionCardinal?[idx] ?? "N/A";
                            var wxPhrase = historicalResponse.WxPhraseLong?[idx] ?? "Unknown";

                            // Try to parse the time string
                            DateTime validTime = DateTime.UtcNow;
                            if (!string.IsNullOrEmpty(timeStr) && DateTime.TryParse(timeStr, out var parsedTime))
                            {
                                validTime = parsedTime;
                            }

                            var weatherModel = new WeatherModel
                            {
                                TimeStamp = validTime,
                                Temperature = (double)temp,
                                WindSpeed = (double)windSpeed,
                                WindDirection = windDir,
                                SkyCondition = wxPhrase,
                                WxPhrase = wxPhrase,
                                Location = "Stigs Bjergby, Denmark"
                            };

                            results.Add(weatherModel);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("No historical data returned or unable to parse response");
                    }
                }

                _logger.LogInformation("Successfully fetched {Count} total historical records", results.Count);
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch historical hourly weather data");
                return results;
            }
        }

        /// <summary>
        /// Fetches historical hourly weather data for a specific date
        /// Uses the v3/wx/conditions/historical/hourly/1day endpoint
        /// https://developer.weather.com/docs/openapi/historical-conditions-hourly-3-0/get-v3-wx-conditions-historical-hourly-1day-by-geocode
        /// </summary>
        /// <summary>
        /// Fetches the 15-minute forecast for the configured location.
        /// https://developer.weather.com/docs/openapi/15-minute-forecast-3-0/get-wx-forecast-fifteenminute
        /// </summary>
        public async Task<List<ForecastSlot>> FetchFifteenMinuteForecastAsync()
        {
            var apiKey = _config["WeatherApi:ApiKey"];
            var lat = _config["WeatherApi:Latitude"] ?? "55.6761";
            var lon = _config["WeatherApi:Longitude"] ?? "12.5683";
            var results = new List<ForecastSlot>();

            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogError("WeatherApi:ApiKey is not configured. Cannot fetch 15-minute forecast.");
                return results;
            }

            try
            {
                var url = $"https://api.weather.com/v3/wx/forecast/fifteenminute?geocode={lat},{lon}&units=m&language=en-US&format=json&apiKey={apiKey}";
                _logger.LogDebug("Fetching 15-minute forecast: {Endpoint}", url.Split("?")[0]);

                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("15-minute forecast endpoint returned {StatusCode}", response.StatusCode);
                    return results;
                }

                var json = await response.Content.ReadAsStringAsync();
                var forecast = JsonSerializer.Deserialize<WeatherApiResponseFifteenMinuteForecast>(json, _jsonOptions);

                if (forecast?.ValidTimeUtc?.Count > 0)
                {
                    for (int i = 0; i < forecast.ValidTimeUtc.Count; i++)
                    {
                        var utcSeconds = forecast.ValidTimeUtc[i];
                        var validTime = utcSeconds.HasValue
                            ? DateTimeOffset.FromUnixTimeSeconds(utcSeconds.Value).UtcDateTime
                            : DateTime.UtcNow;

                        results.Add(new ForecastSlot
                        {
                            ValidTime = validTime,
                            Temperature = forecast.Temperature?.ElementAtOrDefault(i),
                            WindSpeed = forecast.WindSpeed?.ElementAtOrDefault(i),
                            WindDirection = forecast.WindDirectionCardinal?.ElementAtOrDefault(i),
                            WxPhrase = forecast.WxPhraseLong?.ElementAtOrDefault(i),
                            PrecipChance = forecast.PrecipChance?.ElementAtOrDefault(i),
                            CloudCover = forecast.CloudCover?.ElementAtOrDefault(i)
                        });
                    }

                    _logger.LogInformation("Fetched {Count} 15-minute forecast slots", results.Count);
                }
                else
                {
                    _logger.LogWarning("No 15-minute forecast data returned or unable to parse response");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch 15-minute forecast");
            }

            return results;
        }

        public async Task<List<WeatherModel>> FetchHistoricalHourlyAsync(DateTime? specificDate = null)
        {
            var apiKey = _config["WeatherApi:ApiKey"];
            var lat = _config["WeatherApi:Latitude"] ?? "55.6761";
            var lon = _config["WeatherApi:Longitude"] ?? "12.5683";
            var results = new List<WeatherModel>();

            // Check for missing API key early
            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogError("WeatherApi:ApiKey is not configured. Cannot fetch historical data.");
                return results;
            }

            try
            {
                // If no specific date provided, use yesterday
                if (specificDate == null)
                {
                    specificDate = DateTime.UtcNow.AddDays(-1);
                }

                var dateStr = specificDate.Value.ToString("yyyy-MM-dd");
                var url = $"https://api.weather.com/v3/wx/conditions/historical/hourly/1day?geocode={lat},{lon}&apiKey={apiKey}&units=m&language=en-US&format=json";

                _logger.LogDebug("Fetching historical hourly data for {Date}: {Endpoint}", dateStr, url.Split("?")[0]);

                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Historical endpoint returned {StatusCode}", response.StatusCode);
                    return results;
                }

                var json = await response.Content.ReadAsStringAsync();

                // Parse the historical response (arrays format)
                var historicalResponse = JsonSerializer.Deserialize<WeatherApiResponseV3Historical>(json, _jsonOptions);

                if (historicalResponse?.Temperature?.Count > 0)
                {
                    _logger.LogInformation("Fetched {Count} historical hourly records for {Date}", historicalResponse.Temperature.Count, dateStr);

                    // Convert array format to individual WeatherModel records
                    for (int idx = 0; idx < historicalResponse.Temperature.Count; idx++)
                    {
                        var timeStr = historicalResponse.ValidTimeLocal?[idx];
                        var temp = historicalResponse.Temperature?[idx] ?? 0;
                        var windSpeed = historicalResponse.WindSpeed?[idx] ?? 0;
                        var windDir = historicalResponse.WindDirectionCardinal?[idx] ?? "N/A";
                        var wxPhrase = historicalResponse.WxPhraseLong?[idx] ?? "Unknown";

                        // Try to parse the time string
                        DateTime validTime = DateTime.UtcNow;
                        if (!string.IsNullOrEmpty(timeStr) && DateTime.TryParse(timeStr, out var parsedTime))
                        {
                            validTime = parsedTime;
                        }

                        var weatherModel = new WeatherModel
                        {
                            TimeStamp = validTime,
                            Temperature = (double)temp,
                            WindSpeed = (double)windSpeed,
                            WindDirection = windDir,
                            SkyCondition = wxPhrase,
                            WxPhrase = wxPhrase,
                            Location = "Stigs Bjergby, Denmark"
                        };

                        results.Add(weatherModel);
                    }
                }
                else
                {
                    _logger.LogWarning("No historical data returned for {Date} or unable to parse response", dateStr);
                }

                _logger.LogInformation("Successfully fetched {Count} historical records for {Date}", results.Count, dateStr);
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch historical hourly weather data for specific date");
                return results;
            }
        }
    }
}
