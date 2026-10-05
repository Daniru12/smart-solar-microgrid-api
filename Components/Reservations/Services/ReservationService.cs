/*
 * File: ReservationService.cs
 * Author: Upasama (Member 3 - Reservation & Booking Management)
 * Description: FAT Service implementation for all reservation business logic.
 *              All rules enforced here - clients (React web, Kotlin mobile) are UI only.
 *              Business Rules implemented:
 *                1. Reservations must be within 7 days of today (UTC).
 *                2. Updates require at least 12 hours before the reservation.
 *                3. Cancellations require at least 12 hours before the reservation.
 *                4. Status transitions: Pending -> Approved -> Completed / Cancelled.
 *                5. Slot double-booking prevention.
 *              Also implements IReservationChecker for Member 2 cross-component integration.
 * Created: 2026
 */

using SmartSolarMicrogrid.API.Components.Microgrid.Interfaces;
using SmartSolarMicrogrid.API.Components.Reservations.DTOs;
using SmartSolarMicrogrid.API.Components.Reservations.Interfaces;
using SmartSolarMicrogrid.API.Components.Reservations.Models;

namespace SmartSolarMicrogrid.API.Components.Reservations.Services
{
    /// <summary>
    /// Implements IReservationService (business logic for all reservation operations)
    /// and IReservationChecker (cross-component hook for Member 2 station deactivation check).
    /// FAT Service: all validation and business rules live here, not in controllers or clients.
    /// </summary>
    public class ReservationService : IReservationService, IReservationChecker
    {
        private readonly IReservationRepository _repository;
        private readonly IEnergySlotRepository _slotRepository;

        /// <summary>
        /// Constructor: IReservationRepository injected via ASP.NET Core DI container.
        /// </summary>
        public ReservationService(IReservationRepository repository, IEnergySlotRepository slotRepository)
        {
            _repository = repository;
            _slotRepository = slotRepository;
        }

        // ============================================================
        // CROSS-COMPONENT: IReservationChecker (for Member 2)
        // ============================================================

        /// <summary>
        /// Implements IReservationService.HasActiveReservationsForStationAsync.
        /// Returns true if the station has any Pending or Approved reservations.
        /// Used internally and exposed via IReservationService for direct callers.
        /// </summary>
        public async Task<bool> HasActiveReservationsForStationAsync(string stationId)
        {
            return await _repository.HasActiveReservationsForStationAsync(stationId);
        }

        /// <summary>
        /// Implements IReservationChecker.HasActiveReservationsAsync.
        /// Cross-component hook: called by Member 2 MicrogridService to block station deactivation.
        /// Delegates to HasActiveReservationsForStationAsync.
        /// </summary>
        public async Task<bool> HasActiveReservationsAsync(string stationId)
        {
            return await HasActiveReservationsForStationAsync(stationId);
        }

        // ============================================================
        // PROSUMER ACTIONS
        // ============================================================

        /// <summary>
        /// Creates a new reservation.
        /// Business Rules:
        ///   - ReservationDate must be within 7 days from today (UTC).
        ///   - ReservationDate cannot be in the past.
        ///   - Slot must not have a conflicting Pending/Approved reservation on the same date.
        /// Sets Status = Pending, CreatedAt and UpdatedAt = UtcNow.
        /// </summary>
        public async Task<ReservationResponseDto> CreateReservationAsync(CreateReservationDto dto)
        {
            // --- Business Rule 1: 7-Day Booking Window ---
            var now = DateTime.UtcNow;
            if (dto.ReservationDate.Date < now.Date)
                throw new InvalidOperationException("Reservation date cannot be in the past.");

            if (dto.ReservationDate.Date > now.Date.AddDays(7))
                throw new InvalidOperationException(
                    "Reservation must be scheduled within 7 days from today.");

            // --- Business Rule: Ensure slot exists and has enough capacity ---
            var slot = await _slotRepository.GetSlotByIdAsync(dto.SlotId)
                ?? throw new InvalidOperationException("The selected slot does not exist.");

            if (slot.AvailableCapacity < dto.EnergyAmountKwh)
                throw new InvalidOperationException($"Not enough capacity in the slot. Requested: {dto.EnergyAmountKwh} kWh, Available: {slot.AvailableCapacity} kWh.");

            // Build document
            var reservation = new EnergyReservation
            {
                ProsumerNic = dto.ProsumerNic,
                StationId = dto.StationId,
                SlotId = dto.SlotId,
                ReservationDate = dto.ReservationDate.ToUniversalTime(),
                EnergyAmountKwh = dto.EnergyAmountKwh,
                Notes = dto.Notes,
                Status = ReservationStatus.Pending,
                CreatedAt = now,
                UpdatedAt = now,
                // StartTime, EndTime, StationName - populated from Slot/Station lookup if available
                // Left empty here; frontend should display from slot data if needed
                StartTime = string.Empty,
                EndTime = string.Empty,
                StationName = string.Empty
            };

            var created = await _repository.CreateAsync(reservation);

            // Update slot capacity
            slot.AvailableCapacity -= dto.EnergyAmountKwh;
            if (slot.AvailableCapacity <= 0) slot.Status = "Full";
            await _slotRepository.UpdateSlotAsync(slot.SlotId, slot);

            return MapToDto(created);
        }

        /// <summary>
        /// Updates an existing reservation.
        /// Business Rules:
        ///   - At least 12 hours must remain before the current reservation datetime.
        ///   - Only reservations with Status = Pending can be updated.
        ///   - New ReservationDate must also be within 7 days.
        /// </summary>
        public async Task<ReservationResponseDto> UpdateReservationAsync(string id, UpdateReservationDto dto)
        {
            var existing = await _repository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Reservation with ID '{id}' not found.");

            // --- Business Rule: Only Pending reservations can be updated ---
            if (existing.Status != ReservationStatus.Pending)
                throw new InvalidOperationException(
                    $"Only Pending reservations can be updated. Current status: {existing.Status}.");

            // --- Business Rule 2: 12-Hour Update Notice ---
            var hoursUntilReservation = (existing.ReservationDate - DateTime.UtcNow).TotalHours;
            if (hoursUntilReservation < 12)
                throw new InvalidOperationException(
                    "Updates require at least 12 hours' notice before the reservation time. " +
                    $"Only {hoursUntilReservation:F1} hours remain.");

            // --- Business Rule 1: New date must also be within 7 days ---
            var now = DateTime.UtcNow;
            if (dto.ReservationDate.Date < now.Date)
                throw new InvalidOperationException("New reservation date cannot be in the past.");

            if (dto.ReservationDate.Date > now.Date.AddDays(7))
                throw new InvalidOperationException(
                    "New reservation date must be within 7 days from today.");

            // --- Check for slot conflict on new date (skip if slot/date unchanged) ---
            bool slotOrDateChanged = dto.SlotId != existing.SlotId ||
                                     dto.ReservationDate.Date != existing.ReservationDate.Date;
            if (slotOrDateChanged)
            {
                var isConflicting = await _repository.HasConflictingReservationForSlotAsync(
                    dto.SlotId, dto.ReservationDate);
                if (isConflicting)
                    throw new InvalidOperationException(
                        "The new slot is already booked for the selected date.");
            }

            // Handle capacity changes
            var oldSlot = await _slotRepository.GetSlotByIdAsync(existing.SlotId);
            var newSlot = dto.SlotId == existing.SlotId ? oldSlot : await _slotRepository.GetSlotByIdAsync(dto.SlotId);

            if (newSlot == null) throw new InvalidOperationException("The new selected slot does not exist.");

            if (dto.SlotId == existing.SlotId)
            {
                double diff = dto.EnergyAmountKwh - existing.EnergyAmountKwh;
                if (newSlot.AvailableCapacity < diff)
                    throw new InvalidOperationException($"Not enough capacity. Need {diff} more kWh, but only {newSlot.AvailableCapacity} available.");
                
                newSlot.AvailableCapacity -= diff;
                newSlot.Status = newSlot.AvailableCapacity <= 0 ? "Full" : "Available";
                await _slotRepository.UpdateSlotAsync(newSlot.SlotId, newSlot);
            }
            else
            {
                if (newSlot.AvailableCapacity < dto.EnergyAmountKwh)
                    throw new InvalidOperationException($"Not enough capacity in the new slot.");
                
                if (oldSlot != null)
                {
                    oldSlot.AvailableCapacity += existing.EnergyAmountKwh;
                    oldSlot.Status = oldSlot.AvailableCapacity > 0 ? "Available" : "Full";
                    await _slotRepository.UpdateSlotAsync(oldSlot.SlotId, oldSlot);
                }

                newSlot.AvailableCapacity -= dto.EnergyAmountKwh;
                newSlot.Status = newSlot.AvailableCapacity <= 0 ? "Full" : "Available";
                await _slotRepository.UpdateSlotAsync(newSlot.SlotId, newSlot);
            }

            // Apply updates
            existing.SlotId = dto.SlotId;
            existing.ReservationDate = dto.ReservationDate.ToUniversalTime();
            existing.EnergyAmountKwh = dto.EnergyAmountKwh;
            existing.Notes = dto.Notes;
            existing.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(id, existing);
            return MapToDto(existing);
        }

        /// <summary>
        /// Cancels a reservation (soft cancel - sets Status = Cancelled).
        /// Business Rules:
        ///   - At least 12 hours must remain before the reservation.
        ///   - Only Pending or Approved reservations can be cancelled.
        /// </summary>
        public async Task<ReservationResponseDto> CompleteReservationAsync(string id, string? operatorStationId = null)
        {
            var existing = await _repository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Reservation with ID '{id}' not found.");

            if (!string.IsNullOrEmpty(operatorStationId) && existing.StationId != operatorStationId)
                throw new UnauthorizedAccessException("You are not authorized to complete reservations for this station.");

            if (existing.Status != ReservationStatus.Approved)
                throw new InvalidOperationException(
                    $"Only Approved reservations can be completed. Current status: {existing.Status}.");

            existing.Status = ReservationStatus.Completed;
            existing.CompletedAt = DateTime.UtcNow;
            existing.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(id, existing);
            return MapToDto(existing);
        }

        public async Task<ReservationResponseDto> CancelReservationAsync(string id, string? operatorStationId = null)
        {
            var existing = await _repository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Reservation with ID '{id}' not found.");

            if (!string.IsNullOrEmpty(operatorStationId) && existing.StationId != operatorStationId)
                throw new UnauthorizedAccessException("You are not authorized to cancel reservations for this station.");

            // --- Business Rule: Only Pending or Approved can be cancelled ---
            if (existing.Status == ReservationStatus.Cancelled)
                throw new InvalidOperationException("Reservation is already cancelled.");

            if (existing.Status == ReservationStatus.Completed)
                throw new InvalidOperationException("Completed reservations cannot be cancelled.");

            // --- Business Rule 3: 12-Hour Cancellation Notice ---
            var hoursUntilReservation = (existing.ReservationDate - DateTime.UtcNow).TotalHours;
            if (hoursUntilReservation < 12)
                throw new InvalidOperationException(
                    "Cancellations require at least 12 hours' notice before the reservation time. " +
                    $"Only {hoursUntilReservation:F1} hours remain.");

            // Apply cancellation
            existing.Status = ReservationStatus.Cancelled;
            existing.CancelledAt = DateTime.UtcNow;
            existing.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(id, existing);

            // Restore capacity
            var slot = await _slotRepository.GetSlotByIdAsync(existing.SlotId);
            if (slot != null)
            {
                slot.AvailableCapacity += existing.EnergyAmountKwh;
                if (slot.AvailableCapacity > 0) slot.Status = "Available";
                await _slotRepository.UpdateSlotAsync(slot.SlotId, slot);
            }

            return MapToDto(existing);
        }

        // ============================================================
        // BACKOFFICE / GRID OPERATOR ACTIONS
        // ============================================================

        /// <summary>
        /// Validates a QR code for energy transfer.
        /// </summary>
        public async Task<ReservationResponseDto> ValidateQrAsync(string id, string? operatorStationId = null)
        {
            var existing = await _repository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("Invalid QR: Reservation not found.");

            if (!string.IsNullOrEmpty(operatorStationId) && existing.StationId != operatorStationId)
                throw new UnauthorizedAccessException("Invalid QR: You are not authorized to validate reservations for this station.");

            if (existing.Status == ReservationStatus.Cancelled)
                throw new InvalidOperationException("Invalid QR: Reservation is cancelled.");

            if (existing.Status == ReservationStatus.Completed)
                throw new InvalidOperationException("Invalid QR: Energy transfer already completed.");

            if (existing.Status != ReservationStatus.Approved)
                throw new InvalidOperationException($"Invalid QR: Reservation is not approved. Current status: {existing.Status}.");

            return MapToDto(existing);
        }

        /// <summary>
        /// Approves a Pending reservation - sets Status = Approved.
        /// Role enforcement (Backoffice/GridOperator only) is done in the Controller via [Authorize].
        /// </summary>
        public async Task<ReservationResponseDto> ApproveReservationAsync(string id, string? operatorStationId = null)
        {
            var existing = await _repository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Reservation with ID '{id}' not found.");

            if (!string.IsNullOrEmpty(operatorStationId) && existing.StationId != operatorStationId)
                throw new UnauthorizedAccessException("You are not authorized to approve reservations for this station.");

            if (existing.Status != ReservationStatus.Pending)
                throw new InvalidOperationException(
                    $"Only Pending reservations can be approved. Current status: {existing.Status}.");

            existing.Status = ReservationStatus.Approved;
            existing.ApprovedAt = DateTime.UtcNow;
            existing.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(id, existing);
            return MapToDto(existing);
        }

        /// <summary>
        /// Hard deletes a reservation from MongoDB.
        /// Role enforcement (Backoffice only) is done in the Controller via [Authorize].
        /// </summary>
        public async Task DeleteReservationAsync(string id)
        {
            var existing = await _repository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Reservation with ID '{id}' not found.");

            if (existing.Status == ReservationStatus.Pending || existing.Status == ReservationStatus.Approved)
            {
                var slot = await _slotRepository.GetSlotByIdAsync(existing.SlotId);
                if (slot != null)
                {
                    slot.AvailableCapacity += existing.EnergyAmountKwh;
                    if (slot.AvailableCapacity > 0) slot.Status = "Available";
                    await _slotRepository.UpdateSlotAsync(slot.SlotId, slot);
                }
            }

            await _repository.DeleteAsync(id);
        }

        // ============================================================
        // QUERY METHODS
        // ============================================================

        /// <summary>Returns a single reservation by ID. Throws KeyNotFoundException if not found.</summary>
        public async Task<ReservationResponseDto> GetByIdAsync(string id)
        {
            var reservation = await _repository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Reservation with ID '{id}' not found.");
            return MapToDto(reservation);
        }

        /// <summary>Returns all reservations (admin view).</summary>
        public async Task<List<ReservationResponseDto>> GetAllAsync(string? operatorStationId = null)
        {
            var reservations = await _repository.GetAllAsync();
            if (!string.IsNullOrEmpty(operatorStationId))
                reservations = reservations.Where(r => r.StationId == operatorStationId).ToList();
            return reservations.Select(MapToDto).ToList();
        }

        /// <summary>Returns all reservations for a specific Prosumer NIC (booking history).</summary>
        public async Task<List<ReservationResponseDto>> GetByProsumerNicAsync(string nic)
        {
            var reservations = await _repository.GetByProsumerNicAsync(nic);
            return reservations.Select(MapToDto).ToList();
        }

        /// <summary>Returns all Pending reservations sorted oldest first.</summary>
        public async Task<List<ReservationResponseDto>> GetPendingAsync(string? operatorStationId = null)
        {
            var reservations = await _repository.GetPendingAsync();
            if (!string.IsNullOrEmpty(operatorStationId))
                reservations = reservations.Where(r => r.StationId == operatorStationId).ToList();
            return reservations.Select(MapToDto).ToList();
        }

        /// <summary>
        /// Flexible search with optional filters.
        /// Delegates to repository's dynamic MongoDB filter builder.
        /// </summary>
        public async Task<List<ReservationResponseDto>> SearchAsync(ReservationSearchDto searchDto, string? operatorStationId = null)
        {
            var reservations = await _repository.SearchAsync(
                searchDto.Nic,
                searchDto.StationId,
                searchDto.Status,
                searchDto.From,
                searchDto.To);
            
            if (!string.IsNullOrEmpty(operatorStationId))
                reservations = reservations.Where(r => r.StationId == operatorStationId).ToList();

            return reservations.Select(MapToDto).ToList();
        }

        /// <summary>Returns dashboard counts for pending and approved future reservations.</summary>
        public async Task<DashboardSummaryDto> GetDashboardSummaryAsync(string? operatorStationId = null)
        {
            var pendingCount = await _repository.CountPendingAsync(operatorStationId);
            var approvedFutureCount = await _repository.CountApprovedFutureAsync(operatorStationId);

            return new DashboardSummaryDto
            {
                PendingCount = pendingCount,
                ApprovedFutureCount = approvedFutureCount
            };
        }

        // ============================================================
        // PRIVATE HELPER: MapToDto
        // ============================================================

        /// <summary>
        /// Maps an EnergyReservation document to a ReservationResponseDto.
        /// Handles null Id gracefully.
        /// </summary>
        private static ReservationResponseDto MapToDto(EnergyReservation r)
        {
            return new ReservationResponseDto
            {
                Id = r.Id ?? string.Empty,
                ProsumerNic = r.ProsumerNic,
                StationId = r.StationId,
                StationName = r.StationName,
                SlotId = r.SlotId,
                ReservationDate = r.ReservationDate,
                StartTime = r.StartTime,
                EndTime = r.EndTime,
                EnergyAmountKwh = r.EnergyAmountKwh,
                Status = r.Status,
                Notes = r.Notes,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt,
                CancelledAt = r.CancelledAt,
                ApprovedAt = r.ApprovedAt,
                CompletedAt = r.CompletedAt
            };
        }
    }
}


