/*
 * File: ReservationDtos.cs
 * Author: Upasama (Member 3 - Reservation & Booking Management)
 * Description: Data Transfer Objects for the Reservation component.
 *              - CreateReservationDto: input when Prosumer creates a booking.
 *              - UpdateReservationDto: input when Prosumer modifies a booking.
 *              - ReservationResponseDto: output returned to all clients.
 *              - ReservationSearchDto: query parameters for search/filter.
 *              - DashboardSummaryDto: dashboard count data.
 * Created: 2026
 */

using System.ComponentModel.DataAnnotations;

namespace SmartSolarMicrogrid.API.Components.Reservations.DTOs
{
    // ============================================================
    // INPUT DTOs (request body from clients)
    // ============================================================

    /// <summary>
    /// Input DTO for creating a new reservation.
    /// Used by Prosumer via mobile app or web app.
    /// </summary>
    public class CreateReservationDto
    {
        /// <summary>Prosumer NIC - must match the logged-in prosumer (enforced in service).</summary>
        [Required]
        public string ProsumerNic { get; set; } = string.Empty;

        /// <summary>Station ID from Member 2's SolarStationInfo collection.</summary>
        [Required]
        public string StationId { get; set; } = string.Empty;

        /// <summary>Slot ID from Member 2's EnergyBookingSlot collection.</summary>
        [Required]
        public string SlotId { get; set; } = string.Empty;

        /// <summary>
        /// The date on which the reservation is made.
        /// Business Rule: Must be within the next 7 days from today (UTC).
        /// </summary>
        [Required]
        public DateTime ReservationDate { get; set; }

        /// <summary>
        /// Energy amount (kWh) the prosumer wants to trade.
        /// Must be > 0 and <= slot AvailableCapacity.
        /// </summary>
        [Required]
        [Range(0.1, double.MaxValue, ErrorMessage = "Energy amount must be greater than 0.")]
        public double EnergyAmountKwh { get; set; }

        /// <summary>Optional notes from the prosumer.</summary>
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Input DTO for updating an existing reservation.
    /// Business Rule: Only allowed if >= 12 hours remain before the reservation.
    /// </summary>
    public class UpdateReservationDto
    {
        /// <summary>
        /// New reservation date. Must still be within 7 days from today (UTC).
        /// </summary>
        [Required]
        public DateTime ReservationDate { get; set; }

        /// <summary>
        /// Updated slot ID if prosumer wants to switch slots.
        /// </summary>
        [Required]
        public string SlotId { get; set; } = string.Empty;

        /// <summary>Updated energy amount (kWh). Must be > 0.</summary>
        [Required]
        [Range(0.1, double.MaxValue, ErrorMessage = "Energy amount must be greater than 0.")]
        public double EnergyAmountKwh { get; set; }

        /// <summary>Updated optional notes.</summary>
        public string? Notes { get; set; }
    }

    // ============================================================
    // SEARCH DTO (query parameters)
    // ============================================================

    /// <summary>
    /// Optional filters for the search/filter endpoint.
    /// All fields are nullable - omit any field to skip that filter.
    /// </summary>
    public class ReservationSearchDto
    {
        /// <summary>Filter by Prosumer NIC (partial or exact).</summary>
        public string? Nic { get; set; }

        /// <summary>Filter by Station ID.</summary>
        public string? StationId { get; set; }

        /// <summary>Filter by status (Pending/Approved/Cancelled/Completed).</summary>
        public string? Status { get; set; }

        /// <summary>Filter reservations from this date (UTC, inclusive).</summary>
        public DateTime? From { get; set; }

        /// <summary>Filter reservations up to this date (UTC, inclusive).</summary>
        public DateTime? To { get; set; }
    }

    // ============================================================
    // OUTPUT DTOs (response returned to clients)
    // ============================================================

    /// <summary>
    /// Response DTO returned for all reservation read/write operations.
    /// Clients (React web + Kotlin mobile) use this to display reservation info.
    /// </summary>
    public class ReservationResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string ProsumerNic { get; set; } = string.Empty;
        public string StationId { get; set; } = string.Empty;
        public string StationName { get; set; } = string.Empty;
        public string SlotId { get; set; } = string.Empty;
        public DateTime ReservationDate { get; set; }
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public double EnergyAmountKwh { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }

    /// <summary>
    /// Dashboard summary DTO for the web app home/dashboard widget.
    /// Shows pending and upcoming approved reservation counts.
    /// </summary>
    public class DashboardSummaryDto
    {
        /// <summary>Count of reservations with status = Pending.</summary>
        public long PendingCount { get; set; }

        /// <summary>Count of Approved reservations with a future ReservationDate.</summary>
        public long ApprovedFutureCount { get; set; }
    }
}
