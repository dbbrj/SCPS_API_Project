
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices.JavaScript;

namespace SCPS_API_Project.Models
{
    public class WeatherModel
    {
        public int Id { get; set; }
        public DateTime TimeStamp { get; set; }
        public string? jsonString { get; set; }
    }
}
