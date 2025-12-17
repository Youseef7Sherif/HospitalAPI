using Microsoft.EntityFrameworkCore;
using HospitalAPI.Models;

namespace HospitalAPI.Data
{
    public class HospitalContext : DbContext
    {
        public HospitalContext(DbContextOptions<HospitalContext> options) : base(options) { }

        public DbSet<Patients> Patients { get; set; }
        public DbSet<Appointments> Appointments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Patients>(entity =>
            {
                entity.HasKey(e => e.patient_id); // المفتاح الأساسي
                entity.Property(e => e.first_name).HasColumnName("first_name");
                entity.Property(e => e.last_name).HasColumnName("last_name");
                entity.Property(e => e.email).HasColumnName("email");
                entity.Property(e => e.password).HasColumnName("password");
                entity.Property(e => e.address).HasColumnName("address");
                entity.Property(e => e.date_of_birth).HasColumnName("date_of_birth");
                entity.Property(e => e.occupation).HasColumnName("occupation");
                entity.Property(e => e.phone_number).HasColumnName("phone_number");
            });
        }

    }
}
