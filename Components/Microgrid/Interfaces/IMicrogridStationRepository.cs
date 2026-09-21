using SmartSolarMicrogrid.API.Components.Microgrid.Models;

namespace SmartSolarMicrogrid.API.Components.Microgrid.Interfaces
{
    public interface IMicrogridStationRepository
    {
        Task<IEnumerable<SolarStationInfo>> GetAllStationsAsync(bool activeOnly = false);
        Task<SolarStationInfo?> GetStationByIdAsync(string stationId);
        Task<SolarStationInfo> CreateStationAsync(SolarStationInfo station);
        Task<bool> UpdateStationAsync(string stationId, SolarStationInfo stationIn);
        Task<bool> UpdateStationStatusAsync(string stationId, string status);
    }
}
