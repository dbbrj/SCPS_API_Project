using System.ComponentModel.DataAnnotations;

namespace SCPS_API_Project.Models
{
    public class WeatherModel
    {
        public int Id { get; set; }

        [Display(Name = "Time")]
        public DateTime TimeStamp { get; set; }

        [Display(Name = "Temp (°C)")]
        public double Temperature { get; set; }

        [Display(Name = "Wind (km/h)")]
        public double WindSpeed { get; set; }

        [Display(Name = "Wind Dir")]
        public string WindDirection { get; set; } = string.Empty;

        [Display(Name = "Sky")]
        public string SkyCondition { get; set; } = string.Empty;

        [Display(Name = "Weather")]
        public string? WxPhrase { get; set; }

        [Display(Name = "Location")]
        public string Location { get; set; } = string.Empty;
    }
}
