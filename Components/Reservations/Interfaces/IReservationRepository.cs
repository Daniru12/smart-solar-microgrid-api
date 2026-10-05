

using SmartSolarMicrogrid.API.Components.Reservations.Models;

namespace SmartSolarMicrogrid.API.Components.Reservations.Interfaces
{

    public interface IReservationRepository
    {

        Task<List<EnergyReservation>> GetAllAsync();

        Task<EnergyReservation?> GetByIdAsync(string id);

        Task<List<EnergyReservation>> GetByProsumerNicAsync(string nic);

        Task<List<EnergyReservation>> GetByStationIdAsync(string stationId);

        Task<List<EnergyReservation>> GetByStatusAsync(string status);

        Task<List<EnergyReservation>> GetPendingAsync();

        Task<List<EnergyReservation>> SearchAsync(
            string? nic,
            string? stationId,
            string? status,
            DateTime? from,
            DateTime? to);

        Task<bool> HasActiveReservationsForStationAsync(string stationId);

        Task<bool> HasConflictingReservationForSlotAsync(string slotId, DateTime reservationDate);

        Task<long> CountApprovedFutureAsync(string? stationId = null);

        Task<long> CountPendingAsync(string? stationId = null);

        Task<EnergyReservation> CreateAsync(EnergyReservation reservation);

        Task UpdateAsync(string id, EnergyReservation reservation);

        Task DeleteAsync(string id);
    }
}
