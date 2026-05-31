using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SCPS_API_Project.Data;
using SCPS_API_Project.Models;
using SCPS_API_Project.Services;
using System.Text.Json;

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

            // GET: Weather/FetchHistorical — fetches and imports historical hourly data for last N days
            [HttpGet]
            public async Task<IActionResult> FetchHistorical(int days = 1)
            {
                try
                {
                    await _fetchService.FetchAndSaveHistoricalAsync(days);
                    return Json(new { success = true, message = $"Historical data fetch started for {days} day(s)" });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = ex.Message });
                }
            }

            // GET: Weather/DeleteByLocation — deletes all records with a specific location
            [HttpGet]
            public async Task<IActionResult> DeleteByLocation(string location = "Odense, Denmark")
            {
                try
                {
                    int deletedCount = await _fetchService.DeleteRecordsByLocationAsync(location);
                    return Json(new { success = true, deleted = deletedCount, message = $"Deleted {deletedCount} records with location '{location}'" });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = ex.Message });
                }
            }

            // GET: Weather/FetchHistoricalByDate — fetches and imports historical hourly data for a specific date
            [HttpGet]
            public async Task<IActionResult> FetchHistoricalByDate(string date)
            {
                try
                {
                    if (!DateTime.TryParse(date, out var parsedDate))
                    {
                        return Json(new { success = false, message = $"Invalid date format. Use YYYY-MM-DD format. Provided: {date}" });
                    }

                    await _fetchService.FetchAndSaveHistoricalAsync(parsedDate);
                    return Json(new { success = true, message = $"Historical data fetch started for {parsedDate:yyyy-MM-dd}" });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = ex.Message });
                }
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

        // POST: Weather/ImportHistorical — accepts the parallel-array JSON from the historical API
        // and inserts one WeatherModel row per time slot (skips duplicates by UTC timestamp)
        [HttpPost]
        public async Task<IActionResult> ImportHistorical([FromBody] WeatherApiResponseV3Historical data)
        {
            if (data == null || data.ValidTimeUtc.Count == 0)
                return BadRequest(new { error = "No time slots found in the payload." });

            int total = data.ValidTimeUtc.Count;
            var toInsert = new List<WeatherModel>(total);

            for (int i = 0; i < total; i++)
            {
                if (data.ValidTimeUtc[i] is not long utcSeconds) continue;

                var timestamp = DateTimeOffset.FromUnixTimeSeconds(utcSeconds).UtcDateTime;

                bool exists = await _context.WeatherModel.AnyAsync(w => w.TimeStamp == timestamp);
                if (exists) continue;

                toInsert.Add(new WeatherModel
                {
                    TimeStamp = timestamp,
                    Temperature = data.Temperature.ElementAtOrDefault(i) ?? 0,
                    WindSpeed = data.WindSpeed.ElementAtOrDefault(i) ?? 0,
                    WindDirection = data.WindDirectionCardinal.ElementAtOrDefault(i) ?? "N/A",
                    SkyCondition = data.WxPhraseLong.ElementAtOrDefault(i) ?? "Unknown",
                    WxPhrase = data.WxPhraseLong.ElementAtOrDefault(i),
                    Location = "Odense, Denmark"
                });
            }

            _context.WeatherModel.AddRange(toInsert);
            await _context.SaveChangesAsync();

            return Ok(new { inserted = toInsert.Count, skipped = total - toInsert.Count });
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
