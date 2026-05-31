using System.ComponentModel.DataAnnotations;

namespace SCPS_API_Project.Models
{
    public class ForecastModel
    {
        public int Id { get; set; }

        [Display(Name = "Forecast For")]
        public DateTime ValidTime { get; set; }

        [Display(Name = "Fetched At")]
        public DateTime FetchedAt { get; set; }

        [Display(Name = "Temp (°C)")]
        public int? Temperature { get; set; }

        [Display(Name = "Wind (km/h)")]
        public int? WindSpeed { get; set; }

        [Display(Name = "Wind Dir")]
        public string? WindDirection { get; set; }

        [Display(Name = "Condition")]
        public string? WxPhrase { get; set; }

        [Display(Name = "Precip %")]
        public int? PrecipChance { get; set; }

        [Display(Name = "Cloud %")]
        public int? CloudCover { get; set; }
    }
}
