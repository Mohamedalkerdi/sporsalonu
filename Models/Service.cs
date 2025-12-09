using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessCenterApp.Models
{
    public class Service
    {
        public int ServiceId { get; set; }

        [Required(ErrorMessage = "Hizmet adı zorunludur.")]
        public string Name { get; set; } = string.Empty; // Hizmet adı (fitness, yoga, pilates vb.)

        [Required(ErrorMessage = "Hizmet türü zorunludur.")]
        public string ServiceType { get; set; } = string.Empty; // Hizmet türü

        [Required(ErrorMessage = "Süre zorunludur.")]
        [Range(15, 180, ErrorMessage = "Süre 15-180 dakika arasında olmalıdır.")]
        public int Duration { get; set; } // Hizmet süresi (dakika cinsinden)

        [Required(ErrorMessage = "Ücret zorunludur.")]
        [Range(0, 10000, ErrorMessage = "Ücret 0-10000 TL arasında olmalıdır.")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; } // Hizmet ücreti

        public int? FitnessCenterId { get; set; } // Hizmetin bağlı olduğu spor salonu
        public FitnessCenter? FitnessCenter { get; set; } // Hizmetin bağlı olduğu spor salonu

        public ICollection<Appointment> Appointments { get; set; } // Bu hizmete ait randevular

        public Service()
        {
            Appointments = new List<Appointment>();
        }
    }
}

