using FitnessCenterApp.Data;
using FitnessCenterApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FitnessCenterApp.Controllers
{
    [Authorize] // Randevulara sadece giriş yapmış kullanıcılar erişebilir
    public class AppointmentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AppointmentController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Kullanıcı: Kendi randevularını listeleme
        public async Task<IActionResult> MyAppointments()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var appointments = await _context.Appointments
                .Include(a => a.Trainer)
                .Include(a => a.Service)
                .Where(a => a.UserId == user.Id)
                .OrderByDescending(a => a.AppointmentDate)
                .ThenByDescending(a => a.AppointmentTime)
                .ToListAsync();

            return View(appointments);
        }

        // Kullanıcı: Yeni randevu talebi
        [HttpGet]
        public IActionResult Create()
        {
            LoadCreateViewBag(null);
            return View();
        }

        private void LoadCreateViewBag(Appointment? appointment)
        {
            ViewBag.Trainers = new SelectList(_context.Trainers
                .Include(t => t.FitnessCenter)
                .ToList(), "TrainerId", "Name", appointment?.TrainerId);
            ViewBag.Services = new SelectList(_context.Services.ToList(), "ServiceId", "Name", appointment?.ServiceId);
            
            // Servis bilgilerini JSON olarak gönder (JavaScript'te kullanım için)
            var services = _context.Services.Select(s => new { 
                serviceId = s.ServiceId, 
                name = s.Name, 
                price = s.Price, 
                duration = s.Duration 
            }).ToList();
            ViewBag.ServicesJson = JsonSerializer.Serialize(services);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Appointment appointment)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            // ViewBag'leri her zaman yükle (hata durumunda formun düzgün görünmesi için)
            LoadCreateViewBag(appointment);

            // ModelState kontrolü - önce temel validation
            if (!ModelState.IsValid)
            {
                return View(appointment);
            }

            // Randevu çakışma kontrolü
            var hasConflict = await _context.Appointments
                .AnyAsync(a => 
                    a.TrainerId == appointment.TrainerId &&
                    a.AppointmentDate.Date == appointment.AppointmentDate.Date &&
                    a.AppointmentTime == appointment.AppointmentTime &&
                    a.IsConfirmed
                );

            if (hasConflict)
            {
                ModelState.AddModelError("", "Seçilen saatte antrenörün başka bir randevusu bulunmaktadır. Lütfen farklı bir saat seçin.");
                return View(appointment);
            }

            // Antrenör müsaitlik kontrolü
            var trainerAvailability = await _context.TrainerAvailabilities
                .AnyAsync(ta => 
                    ta.TrainerId == appointment.TrainerId &&
                    ta.DayOfWeek == appointment.AppointmentDate.DayOfWeek &&
                    ta.StartTime <= appointment.AppointmentTime &&
                    ta.EndTime >= appointment.AppointmentTime
                );

            if (!trainerAvailability)
            {
                ModelState.AddModelError("", "Seçilen antrenör bu saatte müsait değildir. Lütfen antrenörün müsaitlik saatlerini kontrol edin.");
                return View(appointment);
            }

            // Hizmet bilgilerini al ve toplam ücreti hesapla
            var service = await _context.Services.FindAsync(appointment.ServiceId);
            if (service != null)
            {
                appointment.TotalPrice = service.Price;
            }

            appointment.UserId = user.Id;
            appointment.IsConfirmed = false;

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Randevu talebiniz başarıyla oluşturuldu. Onay bekleniyor.";
            return RedirectToAction(nameof(MyAppointments));
        }

        // Admin: Tüm randevuları listeleme
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var appointments = await _context.Appointments
                .Include(a => a.User)
                .Include(a => a.Trainer)
                .Include(a => a.Service)
                .OrderByDescending(a => a.AppointmentDate)
                .ThenByDescending(a => a.AppointmentTime)
                .ToListAsync();

            return View(appointments);
        }

        // Admin: Randevuyu onaylama
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Approve(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            appointment.IsConfirmed = true;
            _context.Appointments.Update(appointment);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Randevu başarıyla onaylandı.";
            return RedirectToAction(nameof(Index));
        }

        // Admin: Randevuyu reddetme
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Reject(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Randevu başarıyla reddedildi ve silindi.";
            return RedirectToAction(nameof(Index));
        }

        // Kullanıcı: Randevuyu iptal etme
        public async Task<IActionResult> Cancel(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.AppointmentId == id && a.UserId == user.Id);

            if (appointment == null) return NotFound();

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Randevu başarıyla iptal edildi.";
            return RedirectToAction(nameof(MyAppointments));
        }
    }
}
