using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessCenterApp.Models
{
    public class Appointment
    {
        public int AppointmentId { get; set; }
        
        public string? UserId { get; set; } // Randevu talep eden kullanıcı
        public ApplicationUser? User { get; set; }
        
        [Required]
        public int TrainerId { get; set; } // Randevuyu kabul eden antrenör
        public Trainer? Trainer { get; set; } // Navigation property (validation'a dahil edilmez)
        
        [Required]
        public int ServiceId { get; set; } // Seçilen hizmet
        public Service? Service { get; set; } // Navigation property (validation'a dahil edilmez)
        
        [Required(ErrorMessage = "Randevu tarihi zorunludur.")]
        public DateTime AppointmentDate { get; set; } // Randevu tarihi
        
        [Required(ErrorMessage = "Randevu saati zorunludur.")]
        public TimeSpan AppointmentTime { get; set; } // Randevu saati
        
        public bool IsConfirmed { get; set; } // Onay durumu
        
        [Required]
        [Range(0, 10000, ErrorMessage = "Toplam ücret 0-10000 TL arasında olmalıdır.")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalPrice { get; set; } // Toplam ücret
        
        public string? Notes { get; set; } // Ek notlar
    }
}
