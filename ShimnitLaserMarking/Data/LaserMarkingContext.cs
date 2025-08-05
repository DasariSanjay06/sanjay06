using Microsoft.EntityFrameworkCore;
using ShimnitLaserMarking.Models;
using System;

namespace ShimnitLaserMarking.Data
{
    public class LaserMarkingContext : DbContext
    {
        public LaserMarkingContext(DbContextOptions<LaserMarkingContext> options) : base(options)
        {
        }

        public LaserMarkingContext() : base()
        {
        }

        public DbSet<MarkingJob> MarkingJobs { get; set; }
        public DbSet<SystemLog> SystemLogs { get; set; }
        public DbSet<LaserController> LaserControllers { get; set; }
        public DbSet<VehicleType> VehicleTypes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=ShimnitLaserMarkingDB;Trusted_Connection=true;MultipleActiveResultSets=true");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure MarkingJob entity
            modelBuilder.Entity<MarkingJob>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.JobNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.VehicleNumberPlate).IsRequired().HasMaxLength(20);
                entity.Property(e => e.MarkingText).IsRequired();
                entity.Property(e => e.CreatedBy).IsRequired();
                entity.Property(e => e.LaserPower).HasColumnType("decimal(5,2)");
                entity.Property(e => e.MarkingDepth).HasColumnType("decimal(5,3)");
                entity.Property(e => e.PositionX).HasColumnType("decimal(10,3)");
                entity.Property(e => e.PositionY).HasColumnType("decimal(10,3)");
                entity.Property(e => e.PositionZ).HasColumnType("decimal(10,3)");
                entity.Property(e => e.Rotation).HasColumnType("decimal(6,2)");

                entity.HasIndex(e => e.JobNumber).IsUnique();
                entity.HasIndex(e => e.VehicleNumberPlate);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.CreatedDate);
            });

            // Configure SystemLog entity
            modelBuilder.Entity<SystemLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Message).IsRequired();
                entity.Property(e => e.Level).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Source).HasMaxLength(100);
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => e.Level);
            });

            // Configure LaserController entity
            modelBuilder.Entity<LaserController>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
                entity.Property(e => e.SerialNumber).HasMaxLength(100);
                entity.Property(e => e.CommunicationPort).HasMaxLength(20);
                entity.HasIndex(e => e.Type);
                entity.HasIndex(e => e.IsActive);
            });

            // Configure VehicleType entity
            modelBuilder.Entity<VehicleType>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(200);
                entity.HasIndex(e => e.Name).IsUnique();
            });

            // Seed data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Vehicle Types
            modelBuilder.Entity<VehicleType>().HasData(
                new VehicleType { Id = 1, Name = "Car", Description = "Passenger Cars", IsActive = true },
                new VehicleType { Id = 2, Name = "Motorcycle", Description = "Two Wheeler Vehicles", IsActive = true },
                new VehicleType { Id = 3, Name = "Truck", Description = "Commercial Trucks", IsActive = true },
                new VehicleType { Id = 4, Name = "Bus", Description = "Passenger Buses", IsActive = true },
                new VehicleType { Id = 5, Name = "Auto", Description = "Auto Rickshaw", IsActive = true }
            );

            // Seed Laser Controllers
            modelBuilder.Entity<LaserController>().HasData(
                new LaserController 
                { 
                    Id = 1, 
                    Name = "RTC3 Primary", 
                    Type = LaserControllerType.RTC3, 
                    SerialNumber = "RTC3-001", 
                    CommunicationPort = "COM1",
                    IsActive = true,
                    CreatedDate = DateTime.Now
                },
                new LaserController 
                { 
                    Id = 2, 
                    Name = "RTC4 Primary", 
                    Type = LaserControllerType.RTC4, 
                    SerialNumber = "RTC4-001", 
                    CommunicationPort = "COM2",
                    IsActive = true,
                    CreatedDate = DateTime.Now
                }
            );
        }
    }
}