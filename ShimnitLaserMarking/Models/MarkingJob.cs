using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShimnitLaserMarking.Models
{
    public class MarkingJob
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string JobNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string VehicleNumberPlate { get; set; } = string.Empty;

        [StringLength(50)]
        public string VehicleType { get; set; } = string.Empty;

        [StringLength(100)]
        public string OwnerName { get; set; } = string.Empty;

        [Required]
        public string MarkingText { get; set; } = string.Empty;

        [Required]
        public LaserControllerType ControllerType { get; set; }

        public MarkingJobStatus Status { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime? StartedDate { get; set; }
        public DateTime? CompletedDate { get; set; }

        // Laser Parameters
        public double LaserPower { get; set; }
        public int LaserSpeed { get; set; }
        public int PulseFrequency { get; set; }
        public int Passes { get; set; }
        public double MarkingDepth { get; set; }

        // Position Parameters
        public double PositionX { get; set; }
        public double PositionY { get; set; }
        public double PositionZ { get; set; }
        public double Rotation { get; set; }

        // Quality Parameters
        public string? QualityNotes { get; set; }
        public int QualityRating { get; set; } // 1-10 scale

        [StringLength(500)]
        public string? ErrorMessage { get; set; }

        [StringLength(100)]
        public string? OperatorName { get; set; }

        // Audit fields
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime LastModified { get; set; }
        public string? LastModifiedBy { get; set; }
    }

    public enum LaserControllerType
    {
        RTC3 = 1,
        RTC4 = 2
    }

    public enum MarkingJobStatus
    {
        Created = 1,
        Queued = 2,
        InProgress = 3,
        Completed = 4,
        Failed = 5,
        Cancelled = 6
    }
}