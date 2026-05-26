using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SCPS_API_Project.Data;
using SCPS_API_Project.Models;

namespace SCPS_API_Project.Controllers
{
    public class WeatherController : Controller
    {
        private readonly WeatherContext _context;

        public WeatherController(WeatherContext context)
        {
            _context = context;
        }

        // GET: Weather
        public async Task<IActionResult> Index()
        {
            return View(await _context.WeatherModel.ToListAsync());
        }

        // GET: Weather/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var weatherModel = await _context.WeatherModel
                .FirstOrDefaultAsync(m => m.Id == id);
            if (weatherModel == null)
            {
                return NotFound();
            }

            return View(weatherModel);
        }

        // GET: Weather/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Weather/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TimeStamp,jsonString")] WeatherModel weatherModel)
        {
            if (ModelState.IsValid)
            {
                _context.Add(weatherModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(weatherModel);
        }

        // GET: Weather/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var weatherModel = await _context.WeatherModel.FindAsync(id);
            if (weatherModel == null)
            {
                return NotFound();
            }
            return View(weatherModel);
        }

        // POST: Weather/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TimeStamp,jsonString")] WeatherModel weatherModel)
        {
            if (id != weatherModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(weatherModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WeatherModelExists(weatherModel.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(weatherModel);
        }

        // GET: Weather/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var weatherModel = await _context.WeatherModel
                .FirstOrDefaultAsync(m => m.Id == id);
            if (weatherModel == null)
            {
                return NotFound();
            }

            return View(weatherModel);
        }

        // POST: Weather/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var weatherModel = await _context.WeatherModel.FindAsync(id);
            if (weatherModel != null)
            {
                _context.WeatherModel.Remove(weatherModel);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool WeatherModelExists(int id)
        {
            return _context.WeatherModel.Any(e => e.Id == id);
        }
    }
}
