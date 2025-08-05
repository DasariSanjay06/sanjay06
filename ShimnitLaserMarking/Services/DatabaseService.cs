using Microsoft.EntityFrameworkCore;
using Serilog;
using ShimnitLaserMarking.Data;
using ShimnitLaserMarking.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShimnitLaserMarking.Services
{
    public static class DatabaseService
    {
        public static void InitializeDatabase()
        {
            try
            {
                using var context = new LaserMarkingContext();
                
                Log.Information("Initializing database...");
                
                // Ensure database is created
                context.Database.EnsureCreated();
                
                // Apply any pending migrations
                if (context.Database.GetPendingMigrations().Any())
                {
                    Log.Information("Applying database migrations...");
                    context.Database.Migrate();
                }
                
                Log.Information("Database initialization completed successfully");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to initialize database");
                throw;
            }
        }

        public static async Task<List<MarkingJob>> GetMarkingJobsAsync(MarkingJobStatus? status = null, int? limit = null)
        {
            try
            {
                using var context = new LaserMarkingContext();
                var query = context.MarkingJobs.AsQueryable();

                if (status.HasValue)
                {
                    query = query.Where(j => j.Status == status.Value);
                }

                query = query.OrderByDescending(j => j.CreatedDate);

                if (limit.HasValue)
                {
                    query = query.Take(limit.Value);
                }

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error retrieving marking jobs");
                throw;
            }
        }

        public static async Task<MarkingJob?> GetMarkingJobAsync(int id)
        {
            try
            {
                using var context = new LaserMarkingContext();
                return await context.MarkingJobs.FindAsync(id);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error retrieving marking job with ID {JobId}", id);
                throw;
            }
        }

        public static async Task<MarkingJob> CreateMarkingJobAsync(MarkingJob job)
        {
            try
            {
                using var context = new LaserMarkingContext();
                
                // Generate job number if not provided
                if (string.IsNullOrEmpty(job.JobNumber))
                {
                    job.JobNumber = await GenerateJobNumberAsync(context);
                }

                job.CreatedDate = DateTime.Now;
                job.LastModified = DateTime.Now;
                job.Status = MarkingJobStatus.Created;

                context.MarkingJobs.Add(job);
                await context.SaveChangesAsync();

                Log.Information("Created marking job {JobNumber} for vehicle {VehicleNumber}", 
                    job.JobNumber, job.VehicleNumberPlate);

                return job;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error creating marking job");
                throw;
            }
        }

        public static async Task<MarkingJob> UpdateMarkingJobAsync(MarkingJob job)
        {
            try
            {
                using var context = new LaserMarkingContext();
                
                job.LastModified = DateTime.Now;
                
                context.MarkingJobs.Update(job);
                await context.SaveChangesAsync();

                Log.Information("Updated marking job {JobNumber}", job.JobNumber);
                
                return job;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating marking job {JobId}", job.Id);
                throw;
            }
        }

        public static async Task DeleteMarkingJobAsync(int id)
        {
            try
            {
                using var context = new LaserMarkingContext();
                
                var job = await context.MarkingJobs.FindAsync(id);
                if (job != null)
                {
                    context.MarkingJobs.Remove(job);
                    await context.SaveChangesAsync();
                    
                    Log.Information("Deleted marking job {JobNumber}", job.JobNumber);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting marking job {JobId}", id);
                throw;
            }
        }

        public static async Task<List<LaserController>> GetLaserControllersAsync(bool activeOnly = true)
        {
            try
            {
                using var context = new LaserMarkingContext();
                var query = context.LaserControllers.AsQueryable();

                if (activeOnly)
                {
                    query = query.Where(c => c.IsActive);
                }

                return await query.OrderBy(c => c.Name).ToListAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error retrieving laser controllers");
                throw;
            }
        }

        public static async Task<List<VehicleType>> GetVehicleTypesAsync(bool activeOnly = true)
        {
            try
            {
                using var context = new LaserMarkingContext();
                var query = context.VehicleTypes.AsQueryable();

                if (activeOnly)
                {
                    query = query.Where(v => v.IsActive);
                }

                return await query.OrderBy(v => v.Name).ToListAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error retrieving vehicle types");
                throw;
            }
        }

        public static async Task LogSystemEventAsync(string level, string message, string? source = null, Exception? exception = null)
        {
            try
            {
                using var context = new LaserMarkingContext();
                
                var log = new SystemLog
                {
                    Timestamp = DateTime.Now,
                    Level = level,
                    Message = message,
                    Source = source,
                    Exception = exception?.ToString(),
                    UserName = Environment.UserName,
                    MachineName = Environment.MachineName
                };

                context.SystemLogs.Add(log);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Don't throw exceptions from logging to avoid infinite loops
                Log.Error(ex, "Error logging system event to database");
            }
        }

        public static async Task<List<SystemLog>> GetSystemLogsAsync(string? level = null, int? limit = 100)
        {
            try
            {
                using var context = new LaserMarkingContext();
                var query = context.SystemLogs.AsQueryable();

                if (!string.IsNullOrEmpty(level))
                {
                    query = query.Where(l => l.Level == level);
                }

                query = query.OrderByDescending(l => l.Timestamp);

                if (limit.HasValue)
                {
                    query = query.Take(limit.Value);
                }

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error retrieving system logs");
                throw;
            }
        }

        public static async Task CleanupOldLogsAsync(int daysToKeep = 30)
        {
            try
            {
                using var context = new LaserMarkingContext();
                
                var cutoffDate = DateTime.Now.AddDays(-daysToKeep);
                var oldLogs = await context.SystemLogs
                    .Where(l => l.Timestamp < cutoffDate)
                    .ToListAsync();

                if (oldLogs.Any())
                {
                    context.SystemLogs.RemoveRange(oldLogs);
                    await context.SaveChangesAsync();
                    
                    Log.Information("Cleaned up {Count} old log entries", oldLogs.Count);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error cleaning up old logs");
                throw;
            }
        }

        private static async Task<string> GenerateJobNumberAsync(LaserMarkingContext context)
        {
            var today = DateTime.Today;
            var prefix = $"JOB{today:yyyyMMdd}";
            
            var lastJob = await context.MarkingJobs
                .Where(j => j.JobNumber.StartsWith(prefix))
                .OrderByDescending(j => j.JobNumber)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastJob != null && lastJob.JobNumber.Length > prefix.Length)
            {
                var numberPart = lastJob.JobNumber.Substring(prefix.Length);
                if (int.TryParse(numberPart, out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"{prefix}{nextNumber:D4}";
        }
    }
}