using System.ComponentModel.DataAnnotations;

namespace SmartSolarMicrogrid.API.Components.Microgrid.DTOs
{
    public class CreateSlotDto
    {
        [Required]
        public DateTime Date { get; set; }
        
        [Required]
        public string StartTime { get; set; } = string.Empty;
        
        [Required]
        public string EndTime { get; set; } = string.Empty;
        
        [Required]
        [Range(0.1, double.MaxValue)]
        public double Capacity { get; set; }
    }

    public class UpdateSlotDto
    {
        public double Capacity { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class SlotResponseDto
    {
        public string SlotId { get; set; } = string.Empty;
        public string StationId { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public double Capacity { get; set; }
        public double AvailableCapacity { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
