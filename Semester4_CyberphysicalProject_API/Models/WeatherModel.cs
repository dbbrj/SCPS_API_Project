
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices.JavaScript;

namespace Semester4_CyberphysicalProject_API.Models
{
    public class WeatherModel
    {
        public int Id { get; set; }
        public DateTime TimeStamp { get; set; }
        public string? jsonString { get; set; }
    }
}
