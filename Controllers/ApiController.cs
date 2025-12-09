using FitnessCenterApp.Data;
using FitnessCenterApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace FitnessCenterApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Api/Trainers
        // Tüm antrenörleri listeleme
        [HttpGet("Trainers")]
        public IActionResult GetTrainers()
        {
            var trainers = _context.Trainers
                .Include(t => t.FitnessCenter)
                .Select(t => new
                {
                    t.TrainerId,
                    t.Name,
                    t.Expertise,
                    FitnessCenterName = t.FitnessCenter != null ? t.FitnessCenter.Name : "Atanmamış",
                    AppointmentCount = t.Appointments.Count
                })
                .ToList();

            return Ok(trainers);
        }

        // GET: api/Api/Trainers/{fitnessCenterId}
        // Belirli bir spor salonuna ait antrenörleri getirme
        [HttpGet("Trainers/FitnessCenter/{fitnessCenterId}")]
        public IActionResult GetTrainersByFitnessCenter(int fitnessCenterId)
        {
            var trainers = _context.Trainers
                .Where(t => t.FitnessCenterId == fitnessCenterId)
                .Include(t => t.FitnessCenter)
                .Select(t => new
                {
                    t.TrainerId,
                    t.Name,
                    t.Expertise,
                    FitnessCenterName = t.FitnessCenter != null ? t.FitnessCenter.Name : "Atanmamış",
                    AppointmentCount = t.Appointments.Count
                })
                .ToList();

            return Ok(trainers);
        }

        // GET: api/Api/Trainers/Available?date=2024-01-15
        // Belirli bir tarihte uygun antrenörleri getirme
        [HttpGet("Trainers/Available")]
        public IActionResult GetAvailableTrainers([FromQuery] DateTime date)
        {
            if (date == default(DateTime))
            {
                return BadRequest("Tarih parametresi gerekli. Format: yyyy-MM-dd");
            }

            var dayOfWeek = date.DayOfWeek;

            // Belirtilen tarihte müsait olan antrenörleri bul
            var availableTrainers = _context.Trainers
                .Include(t => t.Availabilities)
                .Include(t => t.Appointments)
                .Include(t => t.FitnessCenter)
                .Where(t => t.Availabilities.Any(a => a.DayOfWeek == dayOfWeek))
                .Select(t => new
                {
                    t.TrainerId,
                    t.Name,
                    t.Expertise,
                    FitnessCenterName = t.FitnessCenter != null ? t.FitnessCenter.Name : "Atanmamış",
                    AvailabilityHours = t.Availabilities
                        .Where(a => a.DayOfWeek == dayOfWeek)
                        .Select(a => new { a.StartTime, a.EndTime })
                        .ToList(),
                    HasAppointmentsOnDate = t.Appointments
                        .Any(a => a.AppointmentDate.Date == date.Date)
                })
                .ToList();

            return Ok(availableTrainers);
        }

        // GET: api/Api/Trainers/Available?date=2024-01-15&time=14:00
        // Belirli bir tarih ve saatte uygun antrenörleri getirme
        [HttpGet("Trainers/AvailableAtTime")]
        public IActionResult GetAvailableTrainersAtTime([FromQuery] DateTime date, [FromQuery] TimeSpan time)
        {
            if (date == default(DateTime) || time == default(TimeSpan))
            {
                return BadRequest("Tarih ve saat parametreleri gerekli. Format: date=yyyy-MM-dd&time=HH:mm");
            }

            var dayOfWeek = date.DayOfWeek;

            // Belirtilen tarih ve saatte randevusu olmayan ve müsait olan antrenörleri bul
            var availableTrainers = _context.Trainers
                .Include(t => t.Availabilities)
                .Include(t => t.Appointments)
                .Include(t => t.FitnessCenter)
                .Where(t => 
                    // Müsaitlik saatleri içinde
                    t.Availabilities.Any(a => 
                        a.DayOfWeek == dayOfWeek && 
                        a.StartTime <= time && 
                        a.EndTime >= time
                    ) &&
                    // O saatte randevusu yok
                    !t.Appointments.Any(a => 
                        a.AppointmentDate.Date == date.Date &&
                        a.AppointmentTime == time &&
                        a.IsConfirmed
                    )
                )
                .Select(t => new
                {
                    t.TrainerId,
                    t.Name,
                    t.Expertise,
                    FitnessCenterName = t.FitnessCenter != null ? t.FitnessCenter.Name : "Atanmamış"
                })
                .ToList();

            return Ok(availableTrainers);
        }

        // GET: api/Api/Trainers/{trainerId}/AvailabilityForDate?date=2024-01-15
        // Belirli bir antrenörün, belirli bir gündeki genel müsaitlik aralıklarını ve dolu saatlerini döner
        [HttpGet("Trainers/{trainerId}/AvailabilityForDate")]
        public IActionResult GetTrainerAvailabilityForDate(int trainerId, [FromQuery] DateTime date)
        {
            if (date == default(DateTime))
            {
                return BadRequest("Tarih parametresi gerekli. Format: yyyy-MM-dd");
            }

            var dayOfWeek = date.DayOfWeek;

            // Antrenörün o gündeki tanımlı müsaitlik aralıkları
            var availability = _context.TrainerAvailabilities
                .Where(a => a.TrainerId == trainerId && a.DayOfWeek == dayOfWeek)
                .Select(a => new
                {
                    a.StartTime,
                    a.EndTime
                })
                .ToList();

            // Antrenörün o gündeki onaylı randevuları (dolu saatler)
            var bookedTimes = _context.Appointments
                .Where(a => a.TrainerId == trainerId
                            && a.AppointmentDate.Date == date.Date
                            && a.IsConfirmed)
                .Select(a => a.AppointmentTime)
                .ToList();

            return Ok(new
            {
                TrainerId = trainerId,
                Date = date.Date,
                DayOfWeek = dayOfWeek,
                Availability = availability,
                BookedTimes = bookedTimes
            });
        }

        // GET: api/Api/Appointments/Member/{userId}
        // Üye randevularını getirme
        [HttpGet("Appointments/Member/{userId}")]
        public IActionResult GetMemberAppointments(string userId)
        {
            var appointments = _context.Appointments
                .Include(a => a.Trainer)
                .Include(a => a.Service)
                .Include(a => a.User)
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.AppointmentDate)
                .ThenByDescending(a => a.AppointmentTime)
                .Select(a => new
                {
                    a.AppointmentId,
                    a.AppointmentDate,
                    a.AppointmentTime,
                    TrainerName = a.Trainer.Name,
                    ServiceName = a.Service.Name,
                    ServiceDuration = a.Service.Duration,
                    a.TotalPrice,
                    a.IsConfirmed,
                    a.Notes
                })
                .ToList();

            return Ok(appointments);
        }

        // GET: api/Api/Appointments/Upcoming
        // Yaklaşan randevuları getirme
        [HttpGet("Appointments/Upcoming")]
        public IActionResult GetUpcomingAppointments()
        {
            var now = DateTime.Now;
            
            var appointments = _context.Appointments
                .Include(a => a.Trainer)
                .Include(a => a.Service)
                .Include(a => a.User)
                .Where(a => 
                    a.AppointmentDate.Date >= now.Date &&
                    (a.AppointmentDate.Date > now.Date || 
                     (a.AppointmentDate.Date == now.Date && a.AppointmentTime > TimeSpan.FromHours(now.Hour).Add(TimeSpan.FromMinutes(now.Minute))))
                )
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.AppointmentTime)
                .Select(a => new
                {
                    a.AppointmentId,
                    a.AppointmentDate,
                    a.AppointmentTime,
                    MemberName = a.User != null ? a.User.FullName : "Bilinmiyor",
                    TrainerName = a.Trainer.Name,
                    ServiceName = a.Service.Name,
                    a.TotalPrice,
                    a.IsConfirmed
                })
                .ToList();

            return Ok(appointments);
        }

        // GET: api/Api/Trainers/ByExpertise?expertise=kas geliştirme
        // Uzmanlık alanına göre antrenörleri filtreleme
        [HttpGet("Trainers/ByExpertise")]
        public IActionResult GetTrainersByExpertise([FromQuery] string expertise)
        {
            if (string.IsNullOrEmpty(expertise))
            {
                return BadRequest("Uzmanlık alanı parametresi gerekli.");
            }

            var trainers = _context.Trainers
                .Include(t => t.FitnessCenter)
                .Where(t => t.Expertise.Contains(expertise))
                .Select(t => new
                {
                    t.TrainerId,
                    t.Name,
                    t.Expertise,
                    FitnessCenterName = t.FitnessCenter != null ? t.FitnessCenter.Name : "Atanmamış"
                })
                .ToList();

            return Ok(trainers);
        }

        // GET: api/Api/FitnessCenters
        // Tüm spor salonlarını listeleme
        [HttpGet("FitnessCenters")]
        public IActionResult GetFitnessCenters()
        {
            var fitnessCenters = _context.FitnessCenters
                .Select(f => new
                {
                    f.FitnessCenterId,
                    f.Name,
                    f.Address,
                    f.Phone,
                    TrainerCount = f.Trainers.Count,
                    ServiceCount = f.Services.Count
                })
                .ToList();

            return Ok(fitnessCenters);
        }

        // GET: api/Api/Services
        // Tüm hizmetleri listeleme
        [HttpGet("Services")]
        public IActionResult GetServices()
        {
            var services = _context.Services
                .Include(s => s.FitnessCenter)
                .Select(s => new
                {
                    s.ServiceId,
                    s.Name,
                    s.ServiceType,
                    s.Duration,
                    s.Price,
                    FitnessCenterName = s.FitnessCenter != null ? s.FitnessCenter.Name : "Atanmamış"
                })
                .ToList();

            return Ok(services);
        }

        // GET: api/Api/Services/ByType?type=fitness
        // Hizmet türüne göre filtreleme
        [HttpGet("Services/ByType")]
        public IActionResult GetServicesByType([FromQuery] string type)
        {
            if (string.IsNullOrEmpty(type))
            {
                return BadRequest("Hizmet türü parametresi gerekli.");
            }

            var services = _context.Services
                .Include(s => s.FitnessCenter)
                .Where(s => s.ServiceType.ToLower().Contains(type.ToLower()))
                .Select(s => new
                {
                    s.ServiceId,
                    s.Name,
                    s.ServiceType,
                    s.Duration,
                    s.Price,
                    FitnessCenterName = s.FitnessCenter != null ? s.FitnessCenter.Name : "Atanmamış"
                })
                .ToList();

            return Ok(services);
        }
    }
}

