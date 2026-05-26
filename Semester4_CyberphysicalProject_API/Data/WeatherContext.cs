using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Semester4_CyberphysicalProject_API.Models;

namespace Semester4_CyberphysicalProject_API.Data
{
    public class WeatherContext : DbContext
    {
        public WeatherContext (DbContextOptions<WeatherContext> options)
            : base(options)
        {
        }

        public DbSet<Semester4_CyberphysicalProject_API.Models.WeatherModel> WeatherModel { get; set; } = default!;
    }
}
