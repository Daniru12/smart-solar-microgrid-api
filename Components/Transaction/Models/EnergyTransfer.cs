/*
 * File: EnergyTransfer.cs
 * Description: MongoDB document for a verified QR and the energy-transfer status
 *              of one approved reservation. Stored in the EnergyTransfers collection.
 *              Reservation status itself stays on EnergyReservations (Pending,
 *              Approved, Cancelled, Completed) and is not changed here until the
 *              transfer is marked completed.
 */

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SmartSolarMicrogrid.API.Components.Transaction.Models
{
    public class EnergyTransfer
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string ReservationId { get; set; } = string.Empty;

        public string ProsumerNic { get; set; } = string.Empty;

        public string StationId { get; set; } = string.Empty;

        public string StationName { get; set; } = string.Empty;

        public string SlotId { get; set; } = string.Empty;

        public DateTime ReservationDate { get; set; }

        public string StartTime { get; set; } = string.Empty;

        public string EndTime { get; set; } = string.Empty;

        public double EnergyAmountKwh { get; set; }

        public string QrToken { get; set; } = string.Empty;

        public string QrPayload { get; set; } = string.Empty;

        public DateTime QrIssuedAt { get; set; }

        public string TransferStatus { get; set; } = TransferStatusNames.QrIssued;

        public DateTime? VerifiedAt { get; set; }

        public DateTime? TransferStartedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }

    public static class TransferStatusNames
    {
        public const string QrIssued = "QrIssued";
        public const string InTransfer = "InTransfer";
        public const string Completed = "Completed";
    }
}
