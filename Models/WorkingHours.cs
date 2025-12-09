using System.ComponentModel.DataAnnotations;

namespace FitnessCenterApp.Models
{
    public class WorkingHours
    {
        public int WorkingHoursId { get; set; }

        [Required]
        public DayOfWeek DayOfWeek { get; set; } // Haftanın günü

        [Required]
        public TimeSpan OpeningTime { get; set; } // Açılış saati

        [Required]
        public TimeSpan ClosingTime { get; set; } // Kapanış saati

        public int FitnessCenterId { get; set; } // Çalışma saatlerinin bağlı olduğu spor salonu
        public FitnessCenter FitnessCenter { get; set; } = null!;
    }
}

