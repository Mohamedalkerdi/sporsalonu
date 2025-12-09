using FitnessCenterApp.Data;
using FitnessCenterApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace FitnessCenterApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AppointmentApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/AppointmentApi
        [HttpGet]
        public IActionResult GetAppointments()
        {
            var appointments = _context.Appointments
                .Include(a => a.Trainer)
                .Include(a => a.Service)
                .Include(a => a.User)
                .Select(a => new
                {
                    a.AppointmentId,
                    a.AppointmentDate,
                    a.AppointmentTime,
                    TrainerName = a.Trainer.Name,
                    ServiceName = a.Service.Name,
                    MemberName = a.User != null ? a.User.FullName : "Bilinmiyor",
                    a.IsConfirmed,
                    a.TotalPrice
                }).ToList();

            return Ok(appointments);
        }

        // GET: api/AppointmentApi/{id}
        [HttpGet("{id}")]
        public IActionResult GetAppointment(int id)
        {
            var appointment = _context.Appointments
                .Include(a => a.Trainer)
                .Include(a => a.Service)
                .Include(a => a.User)
                .Where(a => a.AppointmentId == id)
                .Select(a => new
                {
                    a.AppointmentId,
                    a.AppointmentDate,
                    a.AppointmentTime,
                    TrainerName = a.Trainer.Name,
                    ServiceName = a.Service.Name,
                    MemberName = a.User != null ? a.User.FullName : "Bilinmiyor",
                    a.IsConfirmed,
                    a.TotalPrice,
                    a.Notes
                }).FirstOrDefault();

            if (appointment == null)
            {
                return NotFound();
            }

            return Ok(appointment);
        }

        // POST: api/AppointmentApi
        [HttpPost]
        public IActionResult CreateAppointment([FromBody] Appointment appointment)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Appointments.Add(appointment);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetAppointment), new { id = appointment.AppointmentId }, appointment);
        }

        // PUT: api/AppointmentApi/{id}
        [HttpPut("{id}")]
        public IActionResult UpdateAppointment(int id, [FromBody] Appointment updatedAppointment)
        {
            if (id != updatedAppointment.AppointmentId)
            {
                return BadRequest("Appointment ID mismatch.");
            }

            var existingAppointment = _context.Appointments.Find(id);
            if (existingAppointment == null)
            {
                return NotFound();
            }

            existingAppointment.AppointmentDate = updatedAppointment.AppointmentDate;
            existingAppointment.AppointmentTime = updatedAppointment.AppointmentTime;
            existingAppointment.TrainerId = updatedAppointment.TrainerId;
            existingAppointment.ServiceId = updatedAppointment.ServiceId;
            existingAppointment.IsConfirmed = updatedAppointment.IsConfirmed;
            existingAppointment.TotalPrice = updatedAppointment.TotalPrice;
            existingAppointment.Notes = updatedAppointment.Notes;

            _context.SaveChanges();
            return NoContent();
        }

        // DELETE: api/AppointmentApi/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteAppointment(int id)
        {
            var appointment = _context.Appointments.Find(id);
            if (appointment == null)
            {
                return NotFound();
            }

            _context.Appointments.Remove(appointment);
            _context.SaveChanges();
            return NoContent();
        }
    }
}
