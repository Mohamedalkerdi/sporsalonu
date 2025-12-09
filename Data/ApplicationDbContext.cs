using FitnessCenterApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<FitnessCenter> FitnessCenters { get; set; } = null!;
        public DbSet<Service> Services { get; set; } = null!;
        public DbSet<Trainer> Trainers { get; set; } = null!;
        public DbSet<Appointment> Appointments { get; set; } = null!;
        public DbSet<WorkingHours> WorkingHours { get; set; } = null!;
        public DbSet<TrainerAvailability> TrainerAvailabilities { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.User)
                .WithMany(u => u.Appointments)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Trainer)
                .WithMany(t => t.Appointments)
                .HasForeignKey(a => a.TrainerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Service)
                .WithMany(s => s.Appointments)
                .HasForeignKey(a => a.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Trainer>()
                .HasOne(t => t.FitnessCenter)
                .WithMany(f => f.Trainers)
                .HasForeignKey(t => t.FitnessCenterId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Service>()
                .HasOne(s => s.FitnessCenter)
                .WithMany(f => f.Services)
                .HasForeignKey(s => s.FitnessCenterId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<WorkingHours>()
                .HasOne(w => w.FitnessCenter)
                .WithMany(f => f.WorkingHours)
                .HasForeignKey(w => w.FitnessCenterId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TrainerAvailability>()
                .HasOne(ta => ta.Trainer)
                .WithMany(t => t.Availabilities)
                .HasForeignKey(ta => ta.TrainerId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
