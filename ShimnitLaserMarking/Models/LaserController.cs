using System;
using System.ComponentModel.DataAnnotations;

namespace ShimnitLaserMarking.Models
{
    public class LaserController
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public LaserControllerType Type { get; set; }

        [StringLength(100)]
        public string? SerialNumber { get; set; }

        [StringLength(20)]
        public string? CommunicationPort { get; set; }

        public int BaudRate { get; set; } = 115200;

        public bool IsActive { get; set; } = true;

        public bool IsConnected { get; set; } = false;

        public DateTime? LastConnectionDate { get; set; }

        public DateTime CreatedDate { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        // Controller Configuration as JSON
        public string? Configuration { get; set; }

        // Status Information
        public string? LastError { get; set; }
        public DateTime? LastErrorDate { get; set; }

        public int TotalMarkingJobs { get; set; } = 0;
        public double TotalOperatingHours { get; set; } = 0.0;
    }
}