using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SmartSolarMicrogrid.API.Components.Microgrid.Models
{
    public class EnergyBookingSlot
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string SlotId { get; set; } = string.Empty;
        
        public string StationId { get; set; } = string.Empty;
        
        public DateTime Date { get; set; }
        
        public string StartTime { get; set; } = string.Empty;
        
        public string EndTime { get; set; } = string.Empty;
        
        public double Capacity { get; set; }
        
        public double AvailableCapacity { get; set; }
        
        public string Status { get; set; } = "Available"; // Available, Full, Inactive
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime UpdatedAt { get; set; }
    }
}
