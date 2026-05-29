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
    }
}
