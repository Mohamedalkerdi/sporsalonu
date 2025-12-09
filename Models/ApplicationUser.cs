using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace FitnessCenterApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required(ErrorMessage = "Ad soyad alanı zorunludur.")]
        public string FullName { get; set; } = string.Empty;

        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}