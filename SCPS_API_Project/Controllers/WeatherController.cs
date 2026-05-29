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

        // GET: Weather — shows the last 50 snapshots, newest first
        public async Task<IActionResult> Index()
            {
                var snapshots = await _context.WeatherModel
                    .OrderByDescending(w => w.TimeStamp)
                    .Take(50)
                    .ToListAsync();

                return View(snapshots);
            }

            // GET: Weather/DebugApi — test the API endpoint directly
            [HttpGet]
            public async Task<IActionResult> DebugApi()
            {
                var apiService = HttpContext.RequestServices.GetRequiredService<IWeatherApiService>();
                var result = await apiService.FetchCurrentWeatherAsync();

                return Json(new 
                { 
                    success = result != null,
                    data = result,
                    message = result == null ? "Failed to fetch weather data" : "Success"
                });
            }

        // POST: Weather/FetchNow — manually triggers a new snapshot fetch
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FetchNow()
        {
            await _fetchService.FetchAndSaveAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Weather/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var weatherModel = await _context.WeatherModel.FirstOrDefaultAsync(m => m.Id == id);
            if (weatherModel == null) return NotFound();

            return View(weatherModel);
        }

        // GET: Weather/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var weatherModel = await _context.WeatherModel.FirstOrDefaultAsync(m => m.Id == id);
            if (weatherModel == null) return NotFound();

            return View(weatherModel);
        }

        // POST: Weather/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var weatherModel = await _context.WeatherModel.FindAsync(id);
            if (weatherModel != null)
                _context.WeatherModel.Remove(weatherModel);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
