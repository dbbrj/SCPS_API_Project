using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SCPS_API_Project.Data;
using SCPS_API_Project.Models;
using SCPS_API_Project.Services;

namespace SCPS_API_Project.Controllers
{
    public class WeatherController : Controller
    {
        private readonly WeatherContext _context;
        private readonly WeatherFetchService _fetchService;

        public WeatherController(WeatherContext context, WeatherFetchService fetchService)
        {
            _context = context;
            _fetchService = fetchService;
        }

        public async Task<IActionResult> Index()
        {
            var historical = await _context.WeatherModel
                .OrderByDescending(w => w.TimeStamp)
                .Take(100)
                .ToListAsync();

            historical = historical.OrderBy(w => w.TimeStamp).ToList();

            return View(new WeatherIndexViewModel { Historical = historical });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FetchNow()
        {
            await _fetchService.FetchAndSaveAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> DebugApi()
        {
            var apiService = HttpContext.RequestServices.GetRequiredService<IWeatherApiService>();
            var result = await apiService.FetchCurrentWeatherAsync();
            return Json(new
            {
                success = result != null,
                data    = result,
                message = result == null ? "Failed to fetch weather data" : "Success"
            });
        }
    }
}
