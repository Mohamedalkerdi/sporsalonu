using FitnessCenterApp.Data;
using FitnessCenterApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FitnessCenterApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class FitnessCenterController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FitnessCenterController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var fitnessCenters = await _context.FitnessCenters
                .Include(f => f.Trainers)
                .Include(f => f.Services)
                .ToListAsync();
            return View(fitnessCenters);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FitnessCenter fitnessCenter)
        {
            if (ModelState.IsValid)
            {
                _context.FitnessCenters.Add(fitnessCenter);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Spor salonu başarıyla kaydedildi.";
                return RedirectToAction(nameof(Index));
            }
            TempData["ErrorMessage"] = "Bir hata oluştu. Lütfen tekrar deneyin.";
            return View(fitnessCenter);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var fitnessCenter = await _context.FitnessCenters.FindAsync(id);
            if (fitnessCenter == null)
            {
                return NotFound();
            }
            return View(fitnessCenter);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(FitnessCenter fitnessCenter)
        {
            if (ModelState.IsValid)
            {
                _context.FitnessCenters.Update(fitnessCenter);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Spor salonu başarıyla güncellendi.";
                return RedirectToAction(nameof(Index));
            }
            return View(fitnessCenter);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var fitnessCenter = await _context.FitnessCenters.FindAsync(id);
            if (fitnessCenter != null)
            {
                _context.FitnessCenters.Remove(fitnessCenter);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Spor salonu başarıyla silindi.";
            }
            return RedirectToAction(nameof(Index));
        }

        // Çalışma saatleri yönetimi
        public async Task<IActionResult> WorkingHours(int id)
        {
            var fitnessCenter = await _context.FitnessCenters
                .Include(f => f.WorkingHours)
                .FirstOrDefaultAsync(f => f.FitnessCenterId == id);
            
            if (fitnessCenter == null)
            {
                return NotFound();
            }

            ViewBag.FitnessCenterId = id;
            ViewBag.FitnessCenterName = fitnessCenter.Name;
            return View(fitnessCenter.WorkingHours);
        }

        [HttpGet]
        public IActionResult AddWorkingHours(int fitnessCenterId)
        {
            ViewBag.FitnessCenterId = fitnessCenterId;
            ViewBag.DaysOfWeek = new SelectList(Enum.GetValues(typeof(DayOfWeek))
                .Cast<DayOfWeek>()
                .Select(d => new { Value = d, Text = GetDayName(d) }), "Value", "Text");
            
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddWorkingHours(WorkingHours workingHours)
        {
            // FitnessCenterId kontrolü
            if (workingHours.FitnessCenterId <= 0)
            {
                ModelState.AddModelError("FitnessCenterId", "Geçerli bir spor salonu seçilmelidir.");
            }
            else
            {
                var fitnessCenterExists = await _context.FitnessCenters.AnyAsync(f => f.FitnessCenterId == workingHours.FitnessCenterId);
                if (!fitnessCenterExists)
                {
                    ModelState.AddModelError("FitnessCenterId", "Seçilen spor salonu bulunamadı.");
                }
            }

            // Bitiş saati başlangıç saatinden sonra olmalı
            if (workingHours.OpeningTime >= workingHours.ClosingTime)
            {
                ModelState.AddModelError("ClosingTime", "Kapanış saati açılış saatinden sonra olmalıdır.");
            }

            if (ModelState.IsValid)
            {
                _context.WorkingHours.Add(workingHours);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Çalışma saati başarıyla eklendi.";
                return RedirectToAction(nameof(WorkingHours), new { id = workingHours.FitnessCenterId });
            }
            
            ViewBag.FitnessCenterId = workingHours.FitnessCenterId;
            ViewBag.DaysOfWeek = new SelectList(Enum.GetValues(typeof(DayOfWeek))
                .Cast<DayOfWeek>()
                .Select(d => new { Value = d, Text = GetDayName(d) }), "Value", "Text");
            
            return View(workingHours);
        }

        public async Task<IActionResult> DeleteWorkingHours(int id)
        {
            var workingHours = await _context.WorkingHours.FindAsync(id);
            if (workingHours != null)
            {
                var fitnessCenterId = workingHours.FitnessCenterId;
                _context.WorkingHours.Remove(workingHours);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Çalışma saati başarıyla silindi.";
                return RedirectToAction(nameof(WorkingHours), new { id = fitnessCenterId });
            }
            return NotFound();
        }

        private string GetDayName(DayOfWeek day)
        {
            return day switch
            {
                DayOfWeek.Monday => "Pazartesi",
                DayOfWeek.Tuesday => "Salı",
                DayOfWeek.Wednesday => "Çarşamba",
                DayOfWeek.Thursday => "Perşembe",
                DayOfWeek.Friday => "Cuma",
                DayOfWeek.Saturday => "Cumartesi",
                DayOfWeek.Sunday => "Pazar",
                _ => day.ToString()
            };
        }
    }
}

