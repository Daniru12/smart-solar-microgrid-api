/*
 * File: IReservationRepository.cs
 * Author: Upasama (Member 3 - Reservation & Booking Management)
 * Description: Repository interface defining all data access operations
 *              for the EnergyReservation MongoDB collection.
 * Created: 2026
 */

using SmartSolarMicrogrid.API.Components.Reservations.Models;

namespace SmartSolarMicrogrid.API.Components.Reservations.Interfaces
{
    /// <summary>
    /// Defines all MongoDB data access methods for EnergyReservation documents.
    /// Implemented by ReservationRepository. Injected into ReservationService via DI.
    /// </summary>
    public interface IReservationRepository
    {
        // --- Read Operations ---

        /// <summary>Returns all reservations in the collection.</summary>
        Task<List<EnergyReservation>> GetAllAsync();

        /// <summary>Returns a single reservation by its MongoDB ObjectId.</summary>
        Task<EnergyReservation?> GetByIdAsync(string id);

        /// <summary>Returns all reservations belonging to a specific Prosumer by NIC.</summary>
        Task<List<EnergyReservation>> GetByProsumerNicAsync(string nic);

        /// <summary>Returns all reservations for a specific station (used by Member 2 deactivation check).</summary>
        Task<List<EnergyReservation>> GetByStationIdAsync(string stationId);

        /// <summary>Returns all reservations filtered by status (Pending/Approved/Cancelled/Completed).</summary>
        Task<List<EnergyReservation>> GetByStatusAsync(string status);

        /// <summary>Returns all reservations with status = Pending.</summary>
        Task<List<EnergyReservation>> GetPendingAsync();

        /// <summary>
        /// Flexible search with multiple optional filters.
        /// Any null parameter is ignored (not applied as a filter).
        /// </summary>
        Task<List<EnergyReservation>> SearchAsync(
            string? nic,
            string? stationId,
            string? status,
            DateTime? from,
            DateTime? to);

        /// <summary>
        /// Checks whether a station has any active (Pending or Approved) reservations.
        /// Used by Member 2 to block station deactivation.
        /// </summary>
        Task<bool> HasActiveReservationsForStationAsync(string stationId);

        /// <summary>
        /// Checks whether a specific slot already has a conflicting reservation
        /// on the same date (Pending or Approved). Used to prevent double-booking.
        /// </summary>
        Task<bool> HasConflictingReservationForSlotAsync(string slotId, DateTime reservationDate);

        /// <summary>Returns count of Approved future reservations (for dashboard).</summary>
        Task<long> CountApprovedFutureAsync();

        /// <summary>Returns count of Pending reservations (for dashboard).</summary>
        Task<long> CountPendingAsync();

        // --- Write Operations ---

        /// <summary>Inserts a new reservation document into MongoDB.</summary>
        Task<EnergyReservation> CreateAsync(EnergyReservation reservation);

        /// <summary>Replaces an existing reservation document by ID.</summary>
        Task UpdateAsync(string id, EnergyReservation reservation);

        /// <summary>Hard deletes a reservation (admin only, use sparingly).</summary>
        Task DeleteAsync(string id);
    }
}
