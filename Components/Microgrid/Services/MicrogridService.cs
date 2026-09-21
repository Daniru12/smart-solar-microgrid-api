using SmartSolarMicrogrid.API.Components.Microgrid.DTOs;
using SmartSolarMicrogrid.API.Components.Microgrid.Interfaces;
using SmartSolarMicrogrid.API.Components.Microgrid.Models;

namespace SmartSolarMicrogrid.API.Components.Microgrid.Services
{
    public class MicrogridService : IMicrogridService
    {
        private readonly IMicrogridStationRepository _stationRepo;
        private readonly IEnergySlotRepository _slotRepo;
        private readonly IReservationChecker? _reservationChecker;

        public MicrogridService(
            IMicrogridStationRepository stationRepo, 
            IEnergySlotRepository slotRepo,
            IReservationChecker? reservationChecker = null)
        {
            _stationRepo = stationRepo;
            _slotRepo = slotRepo;
            _reservationChecker = reservationChecker;
        }

        public async Task<IEnumerable<StationResponseDto>> GetAllStationsAsync(bool activeOnly = false)
        {
            var stations = await _stationRepo.GetAllStationsAsync(activeOnly);
            return stations.Select(MapToStationResponseDto);
        }

        public async Task<StationResponseDto?> GetStationByIdAsync(string stationId)
        {
            var station = await _stationRepo.GetStationByIdAsync(stationId);
            return station == null ? null : MapToStationResponseDto(station);
        }

        public async Task<StationResponseDto> CreateStationAsync(CreateStationDto createDto)
        {
            var station = new SolarStationInfo
            {
                StationId = Guid.NewGuid().ToString(),
                Name = createDto.Name,
                Address = createDto.Address,
                Latitude = createDto.Latitude,
                Longitude = createDto.Longitude,
                Capacity = createDto.Capacity,
                BatteryCapacity = createDto.BatteryCapacity,
                AvailableStorage = createDto.AvailableStorage,
                OpeningTime = createDto.OpeningTime,
                ClosingTime = createDto.ClosingTime,
                Status = "Active",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var created = await _stationRepo.CreateStationAsync(station);
            return MapToStationResponseDto(created);
        }

        public async Task<bool> UpdateStationAsync(string stationId, UpdateStationDto updateDto)
        {
            var existing = await _stationRepo.GetStationByIdAsync(stationId);
            if (existing == null) return false;

            existing.Name = updateDto.Name;
            existing.Address = updateDto.Address;
            existing.Latitude = updateDto.Latitude;
            existing.Longitude = updateDto.Longitude;
            existing.Capacity = updateDto.Capacity;
            existing.BatteryCapacity = updateDto.BatteryCapacity;
            existing.AvailableStorage = updateDto.AvailableStorage;
            existing.OpeningTime = updateDto.OpeningTime;
            existing.ClosingTime = updateDto.ClosingTime;
            existing.UpdatedAt = DateTime.UtcNow;

            return await _stationRepo.UpdateStationAsync(stationId, existing);
        }

        public async Task<bool> DeactivateStationAsync(string stationId)
        {
            var station = await _stationRepo.GetStationByIdAsync(stationId);
            if (station == null) return false;

            if (_reservationChecker != null)
            {
                bool hasActiveReservations = await _reservationChecker.HasActiveReservationsAsync(stationId);
                if (hasActiveReservations)
                {
                    throw new InvalidOperationException("Cannot deactivate a station with active reservations.");
                }
            }

            return await _stationRepo.UpdateStationStatusAsync(stationId, "Inactive");
        }

        public async Task<bool> ActivateStationAsync(string stationId)
        {
            var station = await _stationRepo.GetStationByIdAsync(stationId);
            if (station == null) return false;

            return await _stationRepo.UpdateStationStatusAsync(stationId, "Active");
        }

        public async Task<IEnumerable<SlotResponseDto>> GetSlotsByStationAsync(string stationId, DateTime? date = null, bool availableOnly = false)
        {
            var slots = await _slotRepo.GetSlotsByStationAsync(stationId, date, availableOnly);
            return slots.Select(MapToSlotResponseDto);
        }

        public async Task<SlotResponseDto?> GetSlotByIdAsync(string slotId)
        {
            var slot = await _slotRepo.GetSlotByIdAsync(slotId);
            return slot == null ? null : MapToSlotResponseDto(slot);
        }

        public async Task<SlotResponseDto> CreateSlotAsync(string stationId, CreateSlotDto createDto)
        {
            var station = await _stationRepo.GetStationByIdAsync(stationId);
            if (station == null || station.Status != "Active")
            {
                throw new InvalidOperationException("Cannot create slot for an invalid or inactive station.");
            }

            var startSpan = TimeSpan.Parse(createDto.StartTime);
            var endSpan = TimeSpan.Parse(createDto.EndTime);
            if (startSpan >= endSpan)
            {
                throw new ArgumentException("StartTime must be before EndTime.");
            }

            // Check overlap
            var existingSlots = await _slotRepo.GetSlotsByStationAndDateAsync(stationId, createDto.Date);
            foreach (var existing in existingSlots)
            {
                var existingStart = TimeSpan.Parse(existing.StartTime);
                var existingEnd = TimeSpan.Parse(existing.EndTime);

                // overlap condition: (StartA < EndB) and (EndA > StartB)
                if (startSpan < existingEnd && endSpan > existingStart)
                {
                    throw new InvalidOperationException("Slot time overlaps with an existing slot.");
                }
            }

            var slot = new EnergyBookingSlot
            {
                SlotId = Guid.NewGuid().ToString(),
                StationId = stationId,
                Date = createDto.Date.Date,
                StartTime = createDto.StartTime,
                EndTime = createDto.EndTime,
                Capacity = createDto.Capacity,
                AvailableCapacity = createDto.Capacity,
                Status = "Available",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var created = await _slotRepo.CreateSlotAsync(slot);
            return MapToSlotResponseDto(created);
        }

        public async Task<bool> UpdateSlotAsync(string slotId, UpdateSlotDto updateDto)
        {
            var existing = await _slotRepo.GetSlotByIdAsync(slotId);
            if (existing == null) return false;

            existing.Capacity = updateDto.Capacity;
            if (!string.IsNullOrEmpty(updateDto.Status))
            {
                existing.Status = updateDto.Status;
            }
            existing.UpdatedAt = DateTime.UtcNow;

            return await _slotRepo.UpdateSlotAsync(slotId, existing);
        }

        public async Task<bool> DeactivateSlotAsync(string slotId)
        {
            var slot = await _slotRepo.GetSlotByIdAsync(slotId);
            if (slot == null) return false;

            return await _slotRepo.UpdateSlotStatusAsync(slotId, "Inactive");
        }

        private static StationResponseDto MapToStationResponseDto(SolarStationInfo station)
        {
            return new StationResponseDto
            {
                StationId = station.StationId,
                Name = station.Name,
                Address = station.Address,
                Latitude = station.Latitude,
                Longitude = station.Longitude,
                Capacity = station.Capacity,
                BatteryCapacity = station.BatteryCapacity,
                AvailableStorage = station.AvailableStorage,
                OpeningTime = station.OpeningTime,
                ClosingTime = station.ClosingTime,
                Status = station.Status,
                CreatedAt = station.CreatedAt
            };
        }

        private static SlotResponseDto MapToSlotResponseDto(EnergyBookingSlot slot)
        {
            return new SlotResponseDto
            {
                SlotId = slot.SlotId,
                StationId = slot.StationId,
                Date = slot.Date,
                StartTime = slot.StartTime,
                EndTime = slot.EndTime,
                Capacity = slot.Capacity,
                AvailableCapacity = slot.AvailableCapacity,
                Status = slot.Status,
                CreatedAt = slot.CreatedAt
            };
        }
    }
}
