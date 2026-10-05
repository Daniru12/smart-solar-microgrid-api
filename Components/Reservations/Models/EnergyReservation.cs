

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SmartSolarMicrogrid.API.Components.Reservations.Models
{

    public class EnergyReservation
    {

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string ProsumerNic { get; set; } = string.Empty;

        public string StationId { get; set; } = string.Empty;

        public string SlotId { get; set; } = string.Empty;

        public DateTime ReservationDate { get; set; }

        public string StartTime { get; set; } = string.Empty;

        public string EndTime { get; set; } = string.Empty;

        public double EnergyAmountKwh { get; set; }

        public string Status { get; set; } = ReservationStatus.Pending;

        public string? Notes { get; set; }

        public string StationName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? CancelledAt { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public DateTime? CompletedAt { get; set; }
    }

    public static class ReservationStatus
    {
        public const string Pending = "Pending";
        public const string Approved = "Approved";
        public const string Cancelled = "Cancelled";
        public const string Completed = "Completed";
    }
}
