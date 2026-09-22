using System.ComponentModel.DataAnnotations;

namespace SmartSolarMicrogrid.API.Components.Microgrid.DTOs
{
    public class CreateStationDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        public string Address { get; set; } = string.Empty;
        
        [Required]
        public double Latitude { get; set; }
        
        [Required]
        public double Longitude { get; set; }
        
        [Required]
        [Range(0, double.MaxValue)]
        public double Capacity { get; set; }
        
        [Required]
        [Range(0, double.MaxValue)]
        public double BatteryCapacity { get; set; }
        
        [Required]
        [Range(0, int.MaxValue)]
        public int AvailableStorage { get; set; }
        
        [Required]
        public string OpeningTime { get; set; } = string.Empty;
        
        [Required]
        public string ClosingTime { get; set; } = string.Empty;
    }

    public class UpdateStationDto
    {
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Capacity { get; set; }
        public double BatteryCapacity { get; set; }
        public int AvailableStorage { get; set; }
        public string OpeningTime { get; set; } = string.Empty;
        public string ClosingTime { get; set; } = string.Empty;
    }

    public class StationResponseDto
    {
        public string StationId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Capacity { get; set; }
        public double BatteryCapacity { get; set; }
        public int AvailableStorage { get; set; }
        public string OpeningTime { get; set; } = string.Empty;
        public string ClosingTime { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
