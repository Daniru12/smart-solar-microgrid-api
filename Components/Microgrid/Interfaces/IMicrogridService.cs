using SmartSolarMicrogrid.API.Components.Microgrid.DTOs;
using SmartSolarMicrogrid.API.Components.Microgrid.Models;

namespace SmartSolarMicrogrid.API.Components.Microgrid.Interfaces
{
    public interface IMicrogridService
    {
        Task<IEnumerable<StationResponseDto>> GetAllStationsAsync(bool activeOnly = false);
        Task<StationResponseDto?> GetStationByIdAsync(string stationId);
        Task<StationResponseDto> CreateStationAsync(CreateStationDto createDto);
        Task<bool> UpdateStationAsync(string stationId, UpdateStationDto updateDto);
        Task<bool> DeactivateStationAsync(string stationId); // Needs to check reservations
        Task<bool> ActivateStationAsync(string stationId);
        
        Task<IEnumerable<SlotResponseDto>> GetSlotsByStationAsync(string stationId, DateTime? date = null, bool availableOnly = false);
        Task<SlotResponseDto?> GetSlotByIdAsync(string slotId);
        Task<SlotResponseDto> CreateSlotAsync(string stationId, CreateSlotDto createDto);
        Task<bool> UpdateSlotAsync(string slotId, UpdateSlotDto updateDto);
        Task<bool> DeactivateSlotAsync(string slotId);
    }
}
