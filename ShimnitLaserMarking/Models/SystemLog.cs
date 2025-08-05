using System;
using System.ComponentModel.DataAnnotations;

namespace ShimnitLaserMarking.Models
{
    public class SystemLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime Timestamp { get; set; }

        [Required]
        [StringLength(20)]
        public string Level { get; set; } = string.Empty;

        [Required]
        public string Message { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Source { get; set; }

        public string? Exception { get; set; }

        [StringLength(100)]
        public string? UserName { get; set; }

        [StringLength(50)]
        public string? MachineName { get; set; }

        public string? Properties { get; set; } // JSON string for additional properties
    }
}