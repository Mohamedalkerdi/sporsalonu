using System.ComponentModel.DataAnnotations;

namespace FitnessCenterApp.Models
{
    public class TrainerAvailability
    {
        public int TrainerAvailabilityId { get; set; }

        [Required]
        public DayOfWeek DayOfWeek { get; set; } // Haftanın günü

        [Required]
        public TimeSpan StartTime { get; set; } // Başlangıç saati

        [Required]
        public TimeSpan EndTime { get; set; } // Bitiş saati

        public int TrainerId { get; set; } // Müsaitliğin bağlı olduğu antrenör
        public Trainer Trainer { get; set; } = null!;
    }
}

