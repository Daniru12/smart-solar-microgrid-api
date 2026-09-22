/*
 * File: EnergyReservation.cs
 * Author: Upasama (Member 3 - Reservation & Booking Management)
 * Description: MongoDB document model for the EnergyReservation collection.
 *              Stores all reservation data linked to a Prosumer (by NIC),
 *              a Solar Station (by StationId), and an Energy Slot (by SlotId).
 * Created: 2026
 */

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SmartSolarMicrogrid.API.Components.Reservations.Models
{
    /// <summary>
    /// Represents a single energy reservation made by a Prosumer for a specific slot
    /// at a solar microgrid station. Status transitions: Pending -> Approved -> Completed.
    /// </summary>
    public class EnergyReservation
    {
        /// <summary>MongoDB primary key (_id).</summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        /// <summary>
        /// Prosumer NIC number (primary key for Prosumer - Member 1).
        /// Used to link this reservation to the prosumer who made it.
        /// </summary>
        public string ProsumerNic { get; set; } = string.Empty;

        /// <summary>
        /// The custom StationId from the SolarStationInfo collection (Member 2).
        /// Format: string identifier set by Microgrid component.
        /// </summary>
        public string StationId { get; set; } = string.Empty;

        /// <summary>
        /// The custom SlotId from the EnergyBookingSlot collection (Member 2).
        /// Format: string identifier set by Microgrid component.
        /// </summary>
        public string SlotId { get; set; } = string.Empty;

        /// <summary>
        /// The date of the reservation. Must be within 7 days from creation time (UTC).
        /// </summary>
        public DateTime ReservationDate { get; set; }

        /// <summary>Snapshot of slot start time at booking time (e.g. "09:00").</summary>
        public string StartTime { get; set; } = string.Empty;

        /// <summary>Snapshot of slot end time at booking time (e.g. "10:00").</summary>
        public string EndTime { get; set; } = string.Empty;

        /// <summary>
        /// Amount of energy (kWh) the prosumer wants to trade/charge.
        /// Cannot exceed the slot AvailableCapacity at time of booking.
        /// </summary>
        public double EnergyAmountKwh { get; set; }

        /// <summary>
        /// Reservation status: Pending | Approved | Cancelled | Completed.
        /// - Pending:   Awaiting Backoffice/GridOperator approval.
        /// - Approved:  Approved by Backoffice or GridOperator.
        /// - Cancelled: Cancelled by Prosumer or Operator (min 12hrs before slot).
        /// - Completed: Energy transfer finalized by Grid Operator QR scan (Member 4).
        /// </summary>
        public string Status { get; set; } = ReservationStatus.Pending;

        /// <summary>Optional notes from the Prosumer at time of booking.</summary>
        public string? Notes { get; set; }

        /// <summary>Station name snapshot for display and history purposes.</summary>
        public string StationName { get; set; } = string.Empty;

        /// <summary>UTC timestamp when reservation was created.</summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>UTC timestamp of the last update to this reservation.</summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>UTC timestamp when the reservation was cancelled. Null if not cancelled.</summary>
        public DateTime? CancelledAt { get; set; }

        /// <summary>UTC timestamp when the reservation was approved. Null if not yet approved.</summary>
        public DateTime? ApprovedAt { get; set; }

        /// <summary>UTC timestamp when the energy transfer was completed. Null if not completed.</summary>
        public DateTime? CompletedAt { get; set; }
    }

    /// <summary>
    /// Static constants for reservation status values.
    /// Using string constants to match MongoDB string storage and JWT claim comparisons.
    /// </summary>
    public static class ReservationStatus
    {
        public const string Pending = "Pending";
        public const string Approved = "Approved";
        public const string Cancelled = "Cancelled";
        public const string Completed = "Completed";
    }
}
