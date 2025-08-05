using System.ComponentModel.DataAnnotations;

namespace ShimnitLaserMarking.Models
{
    public class VehicleType
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        // Default marking parameters for this vehicle type
        public double DefaultPower { get; set; } = 50.0;
        public int DefaultSpeed { get; set; } = 1000;
        public int DefaultPasses { get; set; } = 1;
    }
}