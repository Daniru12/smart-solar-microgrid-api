/*
 * File: ReservationSnapshot.cs
 * Description: Read model for the EnergyReservations collection created by the
 *              booking component. Extra fields are ignored so this component can
 *              read an approved booking before that branch is merged, and can
 *              keep working after it is merged, without replacing the document.
 */

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SmartSolarMicrogrid.API.Components.Transaction.Models
{
    [BsonIgnoreExtraElements]
    public class ReservationSnapshot
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string ProsumerNic { get; set; } = string.Empty;

        public string StationId { get; set; } = string.Empty;

        public string StationName { get; set; } = string.Empty;

        public string SlotId { get; set; } = string.Empty;

        public DateTime ReservationDate { get; set; }

        public string StartTime { get; set; } = string.Empty;

        public string EndTime { get; set; } = string.Empty;

        public double EnergyAmountKwh { get; set; }

        public string Status { get; set; } = string.Empty;
    }

    public static class ReservationStatusNames
    {
        public const string Pending = "Pending";
        public const string Approved = "Approved";
        public const string Cancelled = "Cancelled";
        public const string Completed = "Completed";
    }
}
