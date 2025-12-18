using FitnessCenterApp.Data;
using FitnessCenterApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class TrainerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TrainerController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var trainers = await _context.Trainers
                .Include(t => t.FitnessCenter)
                .ToListAsync();
            return View(trainers);
        }

        [HttpGet]
        public IActionResult Create()
        {
            LoadCreateViewBag(null);
            return View();
        }

        private void LoadCreateViewBag(Trainer? trainer)
        {
            ViewBag.FitnessCenters = new SelectList(_context.FitnessCenters.ToList(), "FitnessCenterId", "Name", trainer?.FitnessCenterId);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Trainer trainer)
        {
            // ViewBag'i her zaman yükle (hata durumunda formun düzgün görünmesi için)
            LoadCreateViewBag(trainer);
            
            if (!ModelState.IsValid)
            {
                return View(trainer);
            }

            _context.Trainers.Add(trainer);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Antrenör başarıyla kaydedildi.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var trainer = await _context.Trainers
                .FirstOrDefaultAsync(t => t.TrainerId == id);

            if (trainer == null)
                return NotFound();

            LoadCreateViewBag(trainer);
            return View(trainer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Trainer trainer)
        {
            if (id != trainer.TrainerId)
                return NotFound();

            // ViewBag'i her zaman yükle (hata durumunda formun düzgün görünmesi için)
            LoadCreateViewBag(trainer);
            
            if (ModelState.IsValid)
            {
                _context.Trainers.Update(trainer);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Antrenör başarıyla güncellendi.";
                return RedirectToAction(nameof(Index));
            }

            return View(trainer);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var trainer = await _context.Trainers.FindAsync(id);
            if (trainer == null) return NotFound();

            _context.Trainers.Remove(trainer);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Antrenör başarıyla silindi.";
            return RedirectToAction(nameof(Index));
        }

        // Antrenör müsaitlik saatleri yönetimi
        public IActionResult Availability(int id)
        {
            var trainer = _context.Trainers
                .Include(t => t.Availabilities)
                .FirstOrDefault(t => t.TrainerId == id);

            if (trainer == null)
            {
                return NotFound();
            }

            ViewBag.TrainerId = id;
            ViewBag.TrainerName = trainer.Name;
            return View(trainer.Availabilities);
        }

        [HttpGet]
        public IActionResult AddAvailability(int trainerId)
        {
            ViewBag.TrainerId = trainerId;
            ViewBag.DaysOfWeek = new SelectList(Enum.GetValues(typeof(DayOfWeek))
                .Cast<DayOfWeek>()
                .Select(d => new { Value = d, Text = GetDayName(d) }), "Value", "Text");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddAvailability(TrainerAvailability availability)
        {
            if (ModelState.IsValid)
            {
                _context.TrainerAvailabilities.Add(availability);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Müsaitlik saati başarıyla eklendi.";
                return RedirectToAction(nameof(Availability), new { id = availability.TrainerId });
            }

            ViewBag.TrainerId = availability.TrainerId;
            ViewBag.DaysOfWeek = new SelectList(Enum.GetValues(typeof(DayOfWeek))
                .Cast<DayOfWeek>()
                .Select(d => new { Value = d, Text = GetDayName(d) }), "Value", "Text");

            return View(availability);
        }

        public IActionResult DeleteAvailability(int id)
        {
            var availability = _context.TrainerAvailabilities.Find(id);
            if (availability != null)
            {
                var trainerId = availability.TrainerId;
                _context.TrainerAvailabilities.Remove(availability);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Müsaitlik saati başarıyla silindi.";
                return RedirectToAction(nameof(Availability), new { id = trainerId });
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

