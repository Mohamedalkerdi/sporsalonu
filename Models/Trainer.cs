using System.ComponentModel.DataAnnotations;

namespace FitnessCenterApp.Models
{
    public class Trainer
    {
        public int TrainerId { get; set; }
        
        [Required(ErrorMessage = "Ad alanı zorunludur.")]
        public string Name { get; set; } = string.Empty; // Antrenör adı
        
        [Required(ErrorMessage = "Uzmanlık alanı zorunludur.")]
        public string Expertise { get; set; } = string.Empty; // Antrenörün uzmanlık alanı (kas geliştirme, kilo verme, yoga vb.)
        
        public int? FitnessCenterId { get; set; } // Antrenörün bağlı olduğu spor salonu
        public FitnessCenter? FitnessCenter { get; set; } // Antrenörün bağlı olduğu spor salonu

        public ICollection<Appointment> Appointments { get; set; } // Antrenörün aldığı randevular
        public ICollection<TrainerAvailability> Availabilities { get; set; } // Antrenörün müsaitlik saatleri

        public Trainer()
        {
            Appointments = new List<Appointment>();
            Availabilities = new List<TrainerAvailability>();
        }
    }
}

