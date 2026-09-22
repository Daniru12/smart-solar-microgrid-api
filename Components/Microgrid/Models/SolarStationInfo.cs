using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SmartSolarMicrogrid.API.Components.Microgrid.Models
{
    public class SolarStationInfo
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

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
        
        public string Status { get; set; } = "Active"; // Active or Inactive
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime UpdatedAt { get; set; }
    }
}
