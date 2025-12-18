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
                .Include(t => t.Appointments)
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
        public async Task<IActionResult> Availability(int id)
        {
            var trainer = await _context.Trainers
                .Include(t => t.Availabilities)
                .FirstOrDefaultAsync(t => t.TrainerId == id);

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
            // TrainerId'yi kontrol et
            if (trainerId <= 0)
            {
                TempData["ErrorMessage"] = "Geçerli bir antrenör seçilmelidir.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.TrainerId = trainerId;
            ViewBag.DaysOfWeek = new SelectList(Enum.GetValues(typeof(DayOfWeek))
                .Cast<DayOfWeek>()
                .Select(d => new { Value = d, Text = GetDayName(d) }), "Value", "Text");

            // Model'i TrainerId ile başlat
            var model = new TrainerAvailability
            {
                TrainerId = trainerId
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAvailability(TrainerAvailability availability)
        {
            // Trainer navigation property validation hatalarını kaldır (sadece TrainerId kullanıyoruz)
            ModelState.Remove("Trainer");

            // TrainerId'yi kontrol et
            if (availability.TrainerId <= 0)
            {
                ModelState.AddModelError("TrainerId", "Geçerli bir antrenör seçilmelidir.");
            }
            else
            {
                // Trainer'ın var olup olmadığını kontrol et
                var trainerExists = await _context.Trainers.AnyAsync(t => t.TrainerId == availability.TrainerId);
                if (!trainerExists)
                {
                    ModelState.AddModelError("TrainerId", "Seçilen antrenör bulunamadı.");
                }
            }

            // Bitiş saati başlangıç saatinden sonra olmalı
            if (availability.StartTime >= availability.EndTime)
            {
                ModelState.AddModelError("EndTime", "Bitiş saati başlangıç saatinden sonra olmalıdır.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Trainer navigation property'yi null yap (sadece TrainerId kullanıyoruz)
                    availability.Trainer = null!;
                    
                    _context.TrainerAvailabilities.Add(availability);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Müsaitlik saati başarıyla eklendi.";
                    return RedirectToAction(nameof(Availability), new { id = availability.TrainerId });
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Müsaitlik saati eklenirken bir hata oluştu: {ex.Message}";
                }
            }

            // ViewBag'leri her zaman set et (validation hatası durumunda formun düzgün görünmesi için)
            ViewBag.TrainerId = availability.TrainerId > 0 ? availability.TrainerId : (ViewBag.TrainerId ?? 0);
            ViewBag.DaysOfWeek = new SelectList(Enum.GetValues(typeof(DayOfWeek))
                .Cast<DayOfWeek>()
                .Select(d => new { Value = d, Text = GetDayName(d) }), "Value", "Text");

            return View(availability);
        }

        public async Task<IActionResult> DeleteAvailability(int id)
        {
            var availability = await _context.TrainerAvailabilities.FindAsync(id);
            if (availability != null)
            {
                var trainerId = availability.TrainerId;
                _context.TrainerAvailabilities.Remove(availability);
                await _context.SaveChangesAsync();
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

