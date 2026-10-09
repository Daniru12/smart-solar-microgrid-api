

using SmartSolarMicrogrid.API.Components.Transaction.Models;

namespace SmartSolarMicrogrid.API.Components.Transaction.Interfaces
{
    public interface IEnergyTransferRepository
    {
        Task<EnergyTransfer?> GetByReservationIdAsync(string reservationId);
        Task<EnergyTransfer> CreateAsync(EnergyTransfer transfer);
        Task<bool> UpdateAsync(EnergyTransfer transfer);
        Task<bool> DeleteByReservationIdAsync(string reservationId);
    }
}
