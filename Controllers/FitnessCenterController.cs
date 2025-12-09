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

        public IActionResult Index()
        {
            var fitnessCenters = _context.FitnessCenters
                .Include(f => f.Trainers)
                .Include(f => f.Services)
                .ToList();
            return View(fitnessCenters);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(FitnessCenter fitnessCenter)
        {
            if (ModelState.IsValid)
            {
                _context.FitnessCenters.Add(fitnessCenter);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Spor salonu başarıyla kaydedildi.";
                return RedirectToAction(nameof(Index));
            }
            TempData["ErrorMessage"] = "Bir hata oluştu. Lütfen tekrar deneyin.";
            return View(fitnessCenter);
        }

        public IActionResult Edit(int id)
        {
            var fitnessCenter = _context.FitnessCenters.Find(id);
            if (fitnessCenter == null)
            {
                return NotFound();
            }
            return View(fitnessCenter);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(FitnessCenter fitnessCenter)
        {
            if (ModelState.IsValid)
            {
                _context.FitnessCenters.Update(fitnessCenter);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Spor salonu başarıyla güncellendi.";
                return RedirectToAction(nameof(Index));
            }
            return View(fitnessCenter);
        }

        public IActionResult Delete(int id)
        {
            var fitnessCenter = _context.FitnessCenters.Find(id);
            if (fitnessCenter != null)
            {
                _context.FitnessCenters.Remove(fitnessCenter);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Spor salonu başarıyla silindi.";
            }
            return RedirectToAction(nameof(Index));
        }

        // Çalışma saatleri yönetimi
        public IActionResult WorkingHours(int id)
        {
            var fitnessCenter = _context.FitnessCenters
                .Include(f => f.WorkingHours)
                .FirstOrDefault(f => f.FitnessCenterId == id);
            
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
        public IActionResult AddWorkingHours(WorkingHours workingHours)
        {
            if (ModelState.IsValid)
            {
                _context.WorkingHours.Add(workingHours);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Çalışma saati başarıyla eklendi.";
                return RedirectToAction(nameof(WorkingHours), new { id = workingHours.FitnessCenterId });
            }
            
            ViewBag.FitnessCenterId = workingHours.FitnessCenterId;
            ViewBag.DaysOfWeek = new SelectList(Enum.GetValues(typeof(DayOfWeek))
                .Cast<DayOfWeek>()
                .Select(d => new { Value = d, Text = GetDayName(d) }), "Value", "Text");
            
            return View(workingHours);
        }

        public IActionResult DeleteWorkingHours(int id)
        {
            var workingHours = _context.WorkingHours.Find(id);
            if (workingHours != null)
            {
                var fitnessCenterId = workingHours.FitnessCenterId;
                _context.WorkingHours.Remove(workingHours);
                _context.SaveChanges();
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

