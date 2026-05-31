using SCPS_API_Project.Data;

namespace SCPS_API_Project.Services
{
    // Singleton background service: automatically fetches a new weather snapshot at a fixed interval
    public class WeatherFetchService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<WeatherFetchService> _logger;
        private readonly TimeSpan _interval;

        public WeatherFetchService(IServiceScopeFactory scopeFactory, ILogger<WeatherFetchService> logger, IConfiguration config)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            var minutes = config.GetValue<int>("WeatherApi:FetchIntervalMinutes", 10);
            _interval = TimeSpan.FromMinutes(minutes);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("WeatherFetchService started. Fetching every {Interval} minutes.", _interval.TotalMinutes);

            // Fetch one snapshot immediately on startup
            await FetchAndSaveAsync();

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(_interval, stoppingToken);
                await FetchAndSaveAsync();
            }
        }

        public async Task FetchAndSaveAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var weatherApi = scope.ServiceProvider.GetRequiredService<IWeatherApiService>();
            var context = scope.ServiceProvider.GetRequiredService<WeatherContext>();

            _logger.LogInformation("Fetching weather snapshot at {Time}", DateTime.UtcNow);

            var snapshot = await weatherApi.FetchCurrentWeatherAsync();
            if (snapshot != null)
            {
                context.WeatherModel.Add(snapshot);
                await context.SaveChangesAsync();
                _logger.LogInformation("Saved: {Temp}°C, Wind {Speed} km/h {Dir}, {Sky}",
                    snapshot.Temperature, snapshot.WindSpeed, snapshot.WindDirection, snapshot.SkyCondition);
            }
        }

        /// <summary>
        /// Fetches historical hourly data for the specified number of days back and saves to database
        /// </summary>
        public async Task FetchAndSaveHistoricalAsync(int daysBack = 1)
        {
            using var scope = _scopeFactory.CreateScope();
            var weatherApi = scope.ServiceProvider.GetRequiredService<IWeatherApiService>();
            var context = scope.ServiceProvider.GetRequiredService<WeatherContext>();

            _logger.LogInformation("Fetching historical hourly data for last {Days} day(s) at {Time}", daysBack, DateTime.UtcNow);

            var historicalData = await weatherApi.FetchHistoricalHourlyAsync(daysBack);

            if (historicalData?.Count > 0)
            {
                // Remove duplicates by checking if record with same timestamp already exists
                var existingTimestamps = context.WeatherModel
                    .AsEnumerable()
                    .Select(w => w.TimeStamp)
                    .ToHashSet();

                var newRecords = historicalData
                    .Where(h => !existingTimestamps.Contains(h.TimeStamp))
                    .ToList();

                if (newRecords.Count > 0)
                {
                    context.WeatherModel.AddRange(newRecords);
                    await context.SaveChangesAsync();
                    _logger.LogInformation("Saved {Count} new historical weather records", newRecords.Count);
                }
                else
                {
                    _logger.LogInformation("All historical records already exist in database");
                }
            }
            else
            {
                _logger.LogWarning("No historical data returned from API");
            }
        }

        /// <summary>
        /// Deletes all weather records with a specific location from the database
        /// </summary>
        public async Task<int> DeleteRecordsByLocationAsync(string location)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<WeatherContext>();

            var recordsToDelete = context.WeatherModel.Where(w => w.Location == location).ToList();
            int count = recordsToDelete.Count;

            if (count > 0)
            {
                context.WeatherModel.RemoveRange(recordsToDelete);
                await context.SaveChangesAsync();
                _logger.LogInformation("Deleted {Count} weather records with location '{Location}'", count, location);
            }
            else
            {
                _logger.LogInformation("No weather records found with location '{Location}'", location);
            }

            return count;
        }

        /// <summary>
        /// Fetches and saves historical weather data for a specific date
        /// </summary>
        public async Task FetchAndSaveHistoricalAsync(DateTime specificDate)
        {
            using var scope = _scopeFactory.CreateScope();
            var weatherApi = scope.ServiceProvider.GetRequiredService<IWeatherApiService>();
            var context = scope.ServiceProvider.GetRequiredService<WeatherContext>();

            _logger.LogInformation("Fetching historical weather data for {Date}", specificDate.ToString("yyyy-MM-dd"));

            var records = await weatherApi.FetchHistoricalHourlyAsync(specificDate);

            if (records?.Count > 0)
            {
                // Check for duplicates based on TimeStamp to avoid re-importing
                var existingTimestamps = new HashSet<DateTime>(
                    context.WeatherModel.Select(w => w.TimeStamp).ToList()
                );

                var newRecords = records.Where(r => !existingTimestamps.Contains(r.TimeStamp)).ToList();

                if (newRecords.Any())
                {
                    context.WeatherModel.AddRange(newRecords);
                    await context.SaveChangesAsync();
                    _logger.LogInformation("Saved {Count} new historical weather records for {Date}", newRecords.Count, specificDate.ToString("yyyy-MM-dd"));
                }
                else
                {
                    _logger.LogInformation("All historical records for {Date} already exist in database", specificDate.ToString("yyyy-MM-dd"));
                }
            }
            else
            {
                _logger.LogWarning("No historical data returned from API for {Date}", specificDate.ToString("yyyy-MM-dd"));
            }
        }
    }
}

