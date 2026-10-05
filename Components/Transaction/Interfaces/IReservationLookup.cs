

using SmartSolarMicrogrid.API.Components.Transaction.Models;

namespace SmartSolarMicrogrid.API.Components.Transaction.Interfaces
{
    public interface IReservationLookup
    {
        Task<ReservationSnapshot?> GetByIdAsync(string reservationId);
        Task<bool> MarkCompletedAsync(string reservationId);
        Task<string?> GetProsumerNicByUserIdAsync(string userId);
    }
}
