using SmartSolarMicrogrid.API.Components.Microgrid.Models;

namespace SmartSolarMicrogrid.API.Components.Microgrid.Interfaces
{
    public interface IEnergySlotRepository
    {
        Task<IEnumerable<EnergyBookingSlot>> GetSlotsByStationAsync(string stationId, DateTime? date = null, bool availableOnly = false);
        Task<EnergyBookingSlot?> GetSlotByIdAsync(string slotId);
        Task<EnergyBookingSlot> CreateSlotAsync(EnergyBookingSlot slot);
        Task<bool> UpdateSlotAsync(string slotId, EnergyBookingSlot slotIn);
        Task<bool> UpdateSlotStatusAsync(string slotId, string status);
        Task<IEnumerable<EnergyBookingSlot>> GetSlotsByStationAndDateAsync(string stationId, DateTime date);
    }
}
