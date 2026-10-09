

using MongoDB.Bson;
using MongoDB.Driver;
using SmartSolarMicrogrid.API.Components.Reservations.Interfaces;
using SmartSolarMicrogrid.API.Components.Reservations.Models;
using SmartSolarMicrogrid.API.Infrastructure.MongoDB;

namespace SmartSolarMicrogrid.API.Components.Reservations.Repositories
{

    public class ReservationRepository : IReservationRepository
    {

        private readonly IMongoCollection<EnergyReservation> _collection;

        public ReservationRepository(MongoDbSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _collection = database.GetCollection<EnergyReservation>("EnergyReservations");
        }

        public async Task<List<EnergyReservation>> GetAllAsync()
        {
            return await _collection.Find(_ => true).ToListAsync();
        }

        public async Task<EnergyReservation?> GetByIdAsync(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId)) return null;
            return await _collection.Find(Builders<EnergyReservation>.Filter.Eq("_id", objectId)).FirstOrDefaultAsync();
        }

        public async Task<List<EnergyReservation>> GetByProsumerNicAsync(string nic)
        {
            return await _collection
                .Find(r => r.ProsumerNic == nic)
                .SortByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<EnergyReservation>> GetByStationIdAsync(string stationId)
        {
            return await _collection
                .Find(r => r.StationId == stationId)
                .SortByDescending(r => r.ReservationDate)
                .ToListAsync();
        }

        public async Task<List<EnergyReservation>> GetByStatusAsync(string status)
        {
            return await _collection
                .Find(r => r.Status == status)
                .SortByDescending(r => r.ReservationDate)
                .ToListAsync();
        }

        public async Task<List<EnergyReservation>> GetPendingAsync()
        {
            return await _collection
                .Find(r => r.Status == ReservationStatus.Pending)
                .SortBy(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<EnergyReservation>> SearchAsync(
            string? nic,
            string? stationId,
            string? status,
            DateTime? from,
            DateTime? to)
        {

            var builder = Builders<EnergyReservation>.Filter;
            var filter = builder.Empty;

            if (!string.IsNullOrWhiteSpace(nic))
                filter &= builder.Eq(r => r.ProsumerNic, nic);

            if (!string.IsNullOrWhiteSpace(stationId))
                filter &= builder.Eq(r => r.StationId, stationId);

            if (!string.IsNullOrWhiteSpace(status))
                filter &= builder.Eq(r => r.Status, status);

            if (from.HasValue)
                filter &= builder.Gte(r => r.ReservationDate, from.Value);

            if (to.HasValue)
                filter &= builder.Lte(r => r.ReservationDate, to.Value.AddDays(1));

            return await _collection
                .Find(filter)
                .SortByDescending(r => r.ReservationDate)
                .ToListAsync();
        }

        public async Task<bool> HasActiveReservationsForStationAsync(string stationId)
        {
            var activeStatuses = new[] { ReservationStatus.Pending, ReservationStatus.Approved };
            var count = await _collection.CountDocumentsAsync(r =>
                r.StationId == stationId && activeStatuses.Contains(r.Status));
            return count > 0;
        }

        public async Task<bool> HasConflictingReservationForSlotAsync(string slotId, DateTime reservationDate)
        {
            var activeStatuses = new[] { ReservationStatus.Pending, ReservationStatus.Approved };
            var date = reservationDate.Date;
            var count = await _collection.CountDocumentsAsync(r =>
                r.SlotId == slotId &&
                r.ReservationDate >= date &&
                r.ReservationDate < date.AddDays(1) &&
                activeStatuses.Contains(r.Status));
            return count > 0;
        }

        public async Task<long> CountApprovedFutureAsync(string? stationId = null)
        {
            return await _collection.CountDocumentsAsync(r =>
                r.Status == ReservationStatus.Approved &&
                r.ReservationDate >= DateTime.UtcNow &&
                (stationId == null || r.StationId == stationId));
        }

        public async Task<long> CountPendingAsync(string? stationId = null)
        {
            return await _collection.CountDocumentsAsync(r =>
                r.Status == ReservationStatus.Pending &&
                (stationId == null || r.StationId == stationId));
        }

        public async Task<EnergyReservation> CreateAsync(EnergyReservation reservation)
        {
            await _collection.InsertOneAsync(reservation);
            return reservation;
        }

        public async Task UpdateAsync(string id, EnergyReservation reservation)
        {
            if (ObjectId.TryParse(id, out var objectId))
            {
                await _collection.ReplaceOneAsync(Builders<EnergyReservation>.Filter.Eq("_id", objectId), reservation);
            }
        }

        public async Task DeleteAsync(string id)
        {
            if (ObjectId.TryParse(id, out var objectId))
            {
                await _collection.DeleteOneAsync(Builders<EnergyReservation>.Filter.Eq("_id", objectId));
            }
        }
    }
}
