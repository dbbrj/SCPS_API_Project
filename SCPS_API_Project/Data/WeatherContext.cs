using Microsoft.EntityFrameworkCore;
using SCPS_API_Project.Models;

namespace SCPS_API_Project.Data
{
    public class WeatherContext : DbContext
    {
        public WeatherContext(DbContextOptions<WeatherContext> options)
            : base(options)
        {
        }

        public DbSet<WeatherModel> WeatherModel { get; set; } = default!;
    }
}
