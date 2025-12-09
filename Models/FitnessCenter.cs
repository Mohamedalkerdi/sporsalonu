using System.ComponentModel.DataAnnotations;

namespace FitnessCenterApp.Models
{
    public class FitnessCenter
    {
        public int FitnessCenterId { get; set; }

        [Required(ErrorMessage = "Spor salonu adı zorunludur.")]
        public string Name { get; set; } = string.Empty; // Spor salonu adı

        [Required(ErrorMessage = "Adres zorunludur.")]
        public string Address { get; set; } = string.Empty; // Adres

        [Required(ErrorMessage = "Telefon numarası zorunludur.")]
        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
        public string Phone { get; set; } = string.Empty; // Telefon

        public ICollection<Trainer> Trainers { get; set; } // Salondaki antrenörler
        public ICollection<Service> Services { get; set; } // Sunulan hizmetler
        public ICollection<WorkingHours> WorkingHours { get; set; } // Çalışma saatleri

        public FitnessCenter()
        {
            Trainers = new List<Trainer>();
            Services = new List<Service>();
            WorkingHours = new List<WorkingHours>();
        }
    }
}

