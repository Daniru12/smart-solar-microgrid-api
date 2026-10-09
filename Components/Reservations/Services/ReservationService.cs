

using SmartSolarMicrogrid.API.Components.Microgrid.Interfaces;
using SmartSolarMicrogrid.API.Components.Reservations.DTOs;
using SmartSolarMicrogrid.API.Components.Reservations.Interfaces;
using SmartSolarMicrogrid.API.Components.Reservations.Models;

namespace SmartSolarMicrogrid.API.Components.Reservations.Services
{

    public class ReservationService : IReservationService, IReservationChecker
    {
        private readonly IReservationRepository _repository;
        private readonly IEnergySlotRepository _slotRepository;

        public ReservationService(IReservationRepository repository, IEnergySlotRepository slotRepository)
        {
            _repository = repository;
            _slotRepository = slotRepository;
        }

        public async Task<bool> HasActiveReservationsForStationAsync(string stationId)
        {
            return await _repository.HasActiveReservationsForStationAsync(stationId);
        }

        public async Task<bool> HasActiveReservationsAsync(string stationId)
        {
            return await HasActiveReservationsForStationAsync(stationId);
        }

        public async Task<ReservationResponseDto> CreateReservationAsync(CreateReservationDto dto)
        {

            var now = DateTime.UtcNow;
            if (dto.ReservationDate.Date < now.Date)
                throw new InvalidOperationException("Reservation date cannot be in the past.");

            if (dto.ReservationDate.Date > now.Date.AddDays(7))
                throw new InvalidOperationException(
                    "Reservation must be scheduled within 7 days from today.");

            var slot = await _slotRepository.GetSlotByIdAsync(dto.SlotId)
                ?? throw new InvalidOperationException("The selected slot does not exist.");

            if (slot.AvailableCapacity < dto.EnergyAmountKwh)
                throw new InvalidOperationException($"Not enough capacity in the slot. Requested: {dto.EnergyAmountKwh} kWh, Available: {slot.AvailableCapacity} kWh.");

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

                StartTime = string.Empty,
                EndTime = string.Empty,
                StationName = string.Empty
            };

            var created = await _repository.CreateAsync(reservation);

            slot.AvailableCapacity -= dto.EnergyAmountKwh;
            if (slot.AvailableCapacity <= 0) slot.Status = "Full";
            await _slotRepository.UpdateSlotAsync(slot.SlotId, slot);

            return MapToDto(created);
        }

        public async Task<ReservationResponseDto> UpdateReservationAsync(string id, UpdateReservationDto dto)
        {
            var existing = await _repository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Reservation with ID '{id}' not found.");

            if (existing.Status != ReservationStatus.Pending)
                throw new InvalidOperationException(
                    $"Only Pending reservations can be updated. Current status: {existing.Status}.");

            var hoursUntilReservation = (existing.ReservationDate - DateTime.UtcNow).TotalHours;
            if (hoursUntilReservation < 12)
                throw new InvalidOperationException(
                    "Updates require at least 12 hours' notice before the reservation time. " +
                    $"Only {hoursUntilReservation:F1} hours remain.");

            var now = DateTime.UtcNow;
            if (dto.ReservationDate.Date < now.Date)
                throw new InvalidOperationException("New reservation date cannot be in the past.");

            if (dto.ReservationDate.Date > now.Date.AddDays(7))
                throw new InvalidOperationException(
                    "New reservation date must be within 7 days from today.");

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

            existing.SlotId = dto.SlotId;
            existing.ReservationDate = dto.ReservationDate.ToUniversalTime();
            existing.EnergyAmountKwh = dto.EnergyAmountKwh;
            existing.Notes = dto.Notes;
            existing.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(id, existing);
            return MapToDto(existing);
        }

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

            if (existing.Status == ReservationStatus.Cancelled)
                throw new InvalidOperationException("Reservation is already cancelled.");

            if (existing.Status == ReservationStatus.Completed)
                throw new InvalidOperationException("Completed reservations cannot be cancelled.");

            var hoursUntilReservation = (existing.ReservationDate - DateTime.UtcNow).TotalHours;
            if (hoursUntilReservation < 12)
                throw new InvalidOperationException(
                    "Cancellations require at least 12 hours' notice before the reservation time. " +
                    $"Only {hoursUntilReservation:F1} hours remain.");

            existing.Status = ReservationStatus.Cancelled;
            existing.CancelledAt = DateTime.UtcNow;
            existing.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(id, existing);

            var slot = await _slotRepository.GetSlotByIdAsync(existing.SlotId);
            if (slot != null)
            {
                slot.AvailableCapacity += existing.EnergyAmountKwh;
                if (slot.AvailableCapacity > 0) slot.Status = "Available";
                await _slotRepository.UpdateSlotAsync(slot.SlotId, slot);
            }

            return MapToDto(existing);
        }

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

        public async Task<ReservationResponseDto> GetByIdAsync(string id)
        {
            var reservation = await _repository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Reservation with ID '{id}' not found.");
            return MapToDto(reservation);
        }

        public async Task<List<ReservationResponseDto>> GetAllAsync(string? operatorStationId = null)
        {
            var reservations = await _repository.GetAllAsync();
            if (!string.IsNullOrEmpty(operatorStationId))
                reservations = reservations.Where(r => r.StationId == operatorStationId).ToList();
            return reservations.Select(MapToDto).ToList();
        }

        public async Task<List<ReservationResponseDto>> GetByProsumerNicAsync(string nic)
        {
            var reservations = await _repository.GetByProsumerNicAsync(nic);
            return reservations.Select(MapToDto).ToList();
        }

        public async Task<List<ReservationResponseDto>> GetPendingAsync(string? operatorStationId = null)
        {
            var reservations = await _repository.GetPendingAsync();
            if (!string.IsNullOrEmpty(operatorStationId))
                reservations = reservations.Where(r => r.StationId == operatorStationId).ToList();
            return reservations.Select(MapToDto).ToList();
        }

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

