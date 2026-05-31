using SCPS_API_Project.Data;

namespace SCPS_API_Project.Services
{
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
            _logger.LogInformation("WeatherFetchService started. Interval: {Interval} min.", _interval.TotalMinutes);

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
            var weatherApi  = scope.ServiceProvider.GetRequiredService<IWeatherApiService>();
            var context     = scope.ServiceProvider.GetRequiredService<WeatherContext>();

            _logger.LogInformation("Fetching current weather snapshot at {Time}", DateTime.UtcNow);

            var snapshot = await weatherApi.FetchCurrentWeatherAsync();
            if (snapshot != null)
            {
                context.WeatherModel.Add(snapshot);
                await context.SaveChangesAsync();
                _logger.LogInformation("Snapshot saved: {Temp}°C, {Wind} km/h {Dir}",
                    snapshot.Temperature, snapshot.WindSpeed, snapshot.WindDirection);
            }
            else
            {
                _logger.LogWarning("Current weather returned null — snapshot not saved");
            }
        }
    }
}
