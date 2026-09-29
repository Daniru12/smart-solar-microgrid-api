/*
 * File: ReservationLookup.cs
 * Description: Reads EnergyReservations and marks one Approved booking Completed
 *              after a verified transfer. Implements IReservationChecker so station
 *              deactivation can see active bookings.
 */

using MongoDB.Driver;
using SmartSolarMicrogrid.API.Components.Identity.Models;
using SmartSolarMicrogrid.API.Components.Microgrid.Interfaces;
using SmartSolarMicrogrid.API.Components.Transaction.Interfaces;
using SmartSolarMicrogrid.API.Components.Transaction.Models;
using SmartSolarMicrogrid.API.Infrastructure.MongoDB;

namespace SmartSolarMicrogrid.API.Components.Transaction.Repositories
{
    public class ReservationLookup : IReservationLookup, IReservationChecker
    {
        private readonly IMongoCollection<ReservationSnapshot> _reservations;
        private readonly IMongoCollection<Prosumer> _prosumers;

        public ReservationLookup(MongoDbSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _reservations = database.GetCollection<ReservationSnapshot>("EnergyReservations");
            _prosumers = database.GetCollection<Prosumer>("Prosumers");
        }

        public async Task<ReservationSnapshot?> GetByIdAsync(string reservationId)
        {
            if (string.IsNullOrWhiteSpace(reservationId))
            {
                return null;
            }

            return await _reservations.Find(r => r.Id == reservationId).FirstOrDefaultAsync();
        }

        public async Task<bool> MarkCompletedAsync(string reservationId)
        {
            var update = Builders<ReservationSnapshot>.Update
                .Set(r => r.Status, ReservationStatusNames.Completed)
                .Set("CompletedAt", DateTime.UtcNow)
                .Set("UpdatedAt", DateTime.UtcNow);

            var result = await _reservations.UpdateOneAsync(
                r => r.Id == reservationId && r.Status == ReservationStatusNames.Approved,
                update);

            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> HasActiveReservationsAsync(string stationId)
        {
            var filter = Builders<ReservationSnapshot>.Filter.Eq(r => r.StationId, stationId)
                & Builders<ReservationSnapshot>.Filter.In(r => r.Status, new[]
                {
                    ReservationStatusNames.Pending,
                    ReservationStatusNames.Approved
                });

            return await _reservations.Find(filter).AnyAsync();
        }

        public async Task<string?> GetProsumerNicByUserIdAsync(string userId)
        {
            var prosumer = await _prosumers.Find(p => p.UserId == userId).FirstOrDefaultAsync();
            return prosumer?.NIC;
        }
    }
}
