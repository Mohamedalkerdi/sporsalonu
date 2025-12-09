using FitnessCenterApp.Data;
using FitnessCenterApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ServiceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ServiceController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var services = await _context.Services
                .Include(s => s.FitnessCenter)
                .ToListAsync();
            return View(services);
        }

        [HttpGet]
        public IActionResult Create()
        {
            LoadCreateViewBag(null);
            return View();
        }

        private void LoadCreateViewBag(Service? service)
        {
            ViewBag.FitnessCenters = new SelectList(_context.FitnessCenters.ToList(), "FitnessCenterId", "Name", service?.FitnessCenterId);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Service service)
        {
            // ViewBag'i her zaman yükle (hata durumunda formun düzgün görünmesi için)
            LoadCreateViewBag(service);
            
            if (!ModelState.IsValid)
            {
                return View(service);
            }

            _context.Services.Add(service);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Hizmet başarıyla kaydedildi.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var service = await _context.Services
                .FirstOrDefaultAsync(s => s.ServiceId == id);

            if (service == null)
                return NotFound();

            LoadCreateViewBag(service);
            return View(service);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Service service)
        {
            if (id != service.ServiceId)
                return NotFound();

            // ViewBag'i her zaman yükle (hata durumunda formun düzgün görünmesi için)
            LoadCreateViewBag(service);
            
            if (ModelState.IsValid)
            {
                _context.Services.Update(service);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Hizmet başarıyla güncellendi.";
                return RedirectToAction(nameof(Index));
            }

            return View(service);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null) return NotFound();

            _context.Services.Remove(service);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Hizmet başarıyla silindi.";
            return RedirectToAction(nameof(Index));
        }
    }
}

