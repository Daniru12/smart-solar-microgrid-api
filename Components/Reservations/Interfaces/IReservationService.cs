/*
 * File: IReservationService.cs
 * Author: Upasama (Member 3 - Reservation & Booking Management)
 * Description: Service interface defining all business-logic operations
 *              for the Reservation component. All rules (7-day, 12-hour)
 *              are enforced here, not in the Controller or clients.
 * Created: 2026
 */

using SmartSolarMicrogrid.API.Components.Reservations.DTOs;

namespace SmartSolarMicrogrid.API.Components.Reservations.Interfaces
{
    /// <summary>
    /// Defines all business-logic methods for reservation management.
    /// Implemented by ReservationService. Injected into ReservationController via DI.
    /// FAT Service pattern: all business rules live here.
    /// </summary>
    public interface IReservationService
    {
        // --- Prosumer Actions ---

        /// <summary>
        /// Creates a new reservation. Enforces:
        ///   - Reservation date must be within 7 days.
        ///   - Slot must not already be booked for that date.
        ///   - Slot must be Active and have sufficient AvailableCapacity.
        /// </summary>
        Task<ReservationResponseDto> CreateReservationAsync(CreateReservationDto dto);

        /// <summary>
        /// Updates an existing reservation (date, time, energy amount).
        /// Enforces: At least 12 hours must remain before the reservation date.
        /// Only Pending reservations can be updated.
        /// </summary>
        Task<ReservationResponseDto> UpdateReservationAsync(string id, UpdateReservationDto dto);

        /// <summary>
        /// Cancels a reservation. Enforces:
        ///   - At least 12 hours must remain before the reservation date.
        ///   - Only Pending or Approved reservations can be cancelled.
        /// </summary>
        Task<ReservationResponseDto> CancelReservationAsync(string id);

        // --- Backoffice / GridOperator Actions ---

        /// <summary>
        /// Approves a Pending reservation. Sets status to Approved.
        /// Only accessible by Backoffice or GridOperator roles.
        /// </summary>
        Task<ReservationResponseDto> ApproveReservationAsync(string id);

        /// <summary>
        /// Completes an Approved reservation (e.g. after QR scan). Sets status to Completed.
        /// </summary>
        Task<ReservationResponseDto> CompleteReservationAsync(string id);

        /// <summary>
        /// Hard deletes a reservation from the database.
        /// Only accessible by Backoffice role.
        /// </summary>
        Task DeleteReservationAsync(string id);

        // --- Query Methods ---

        /// <summary>Returns a single reservation by ID. Throws if not found.</summary>
        Task<ReservationResponseDto> GetByIdAsync(string id);

        /// <summary>Returns all reservations (admin view).</summary>
        Task<List<ReservationResponseDto>> GetAllAsync();

        /// <summary>Returns all reservations for a specific Prosumer NIC (booking history).</summary>
        Task<List<ReservationResponseDto>> GetByProsumerNicAsync(string nic);

        /// <summary>Returns all reservations with Pending status.</summary>
        Task<List<ReservationResponseDto>> GetPendingAsync();

        /// <summary>
        /// Searches reservations with optional filters.
        /// All parameters are optional - null means "no filter applied".
        /// </summary>
        Task<List<ReservationResponseDto>> SearchAsync(ReservationSearchDto searchDto);

        /// <summary>Returns dashboard summary counts.</summary>
        Task<DashboardSummaryDto> GetDashboardSummaryAsync();

        // --- Cross-component integration (for Member 2 IReservationChecker) ---

        /// <summary>
        /// Checks if any active (Pending or Approved) reservations exist for a station.
        /// Called by Member 2's MicrogridService to block station deactivation.
        /// </summary>
        Task<bool> HasActiveReservationsForStationAsync(string stationId);
    }
}

