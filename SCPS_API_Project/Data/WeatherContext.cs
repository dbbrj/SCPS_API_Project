using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SCPS_API_Project.Models;

namespace SCPS_API_Project.Data
{
    public class WeatherContext : DbContext
    {
        public WeatherContext (DbContextOptions<WeatherContext> options)
            : base(options)
        {
        }

        public DbSet<SCPS_API_Project.Models.WeatherModel> WeatherModel { get; set; } = default!;
        public DbSet<SCPS_API_Project.Models.ForecastModel> ForecastModel { get; set; } = default!;
    }
}
