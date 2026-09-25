/*
 * File: IReservationLookup.cs
 * Description: Reads and completes bookings in EnergyReservations without owning
 *              create, update, or approve. Also answers whether a station still
 *              has an active booking so a station cannot be deactivated.
 */

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
