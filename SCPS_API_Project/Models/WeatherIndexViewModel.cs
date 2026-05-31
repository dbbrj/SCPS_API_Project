namespace SCPS_API_Project.Models
{
    public class WeatherIndexViewModel
    {
        public IEnumerable<WeatherModel> Historical { get; set; } = [];
        public IEnumerable<ForecastModel> Forecast { get; set; } = [];
    }

    // Intermediate DTO returned by WeatherApiService before the data is persisted as ForecastModel
    public class ForecastSlot
    {
        public DateTime ValidTime { get; set; }
        public int? Temperature { get; set; }
        public int? WindSpeed { get; set; }
        public string? WindDirection { get; set; }
        public string? WxPhrase { get; set; }
        public int? PrecipChance { get; set; }
        public int? CloudCover { get; set; }
    }
}
