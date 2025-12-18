using System.ComponentModel.DataAnnotations;

namespace FitnessCenterApp.Models
{
    public class TrainerAvailability
    {
        public int TrainerAvailabilityId { get; set; }

        [Required(ErrorMessage = "Gün seçimi zorunludur.")]
        public DayOfWeek DayOfWeek { get; set; } // Haftanın günü

        [Required(ErrorMessage = "Başlangıç saati zorunludur.")]
        public TimeSpan StartTime { get; set; } // Başlangıç saati

        [Required(ErrorMessage = "Bitiş saati zorunludur.")]
        public TimeSpan EndTime { get; set; } // Bitiş saati

        [Required(ErrorMessage = "Antrenör seçimi zorunludur.")]
        public int TrainerId { get; set; } // Müsaitliğin bağlı olduğu antrenör
        public Trainer Trainer { get; set; } = null!;
    }
}

