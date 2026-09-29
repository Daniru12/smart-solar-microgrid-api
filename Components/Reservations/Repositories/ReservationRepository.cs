/*
 * File: ReservationRepository.cs
 * Author: Upasama (Member 3 - Reservation & Booking Management)
 * Description: MongoDB repository implementation for the EnergyReservation collection.
 *              All database read/write operations are handled here.
 *              No business logic - only data access.
 * Created: 2026
 */

using MongoDB.Bson;
using MongoDB.Driver;
using SmartSolarMicrogrid.API.Components.Reservations.Interfaces;
using SmartSolarMicrogrid.API.Components.Reservations.Models;
using SmartSolarMicrogrid.API.Infrastructure.MongoDB;

namespace SmartSolarMicrogrid.API.Components.Reservations.Repositories
{
    /// <summary>
    /// Implements IReservationRepository using the MongoDB .NET Driver.
    /// Accesses the "EnergyReservations" collection in the shared MongoDB database.
    /// </summary>
    public class ReservationRepository : IReservationRepository
    {
        // MongoDB collection reference - injected via MongoDbSettings singleton
        private readonly IMongoCollection<EnergyReservation> _collection;

        /// <summary>
        /// Constructor: retrieves the EnergyReservations collection from MongoDB.
        /// Collection name is hardcoded to "EnergyReservations".
        /// </summary>
        public ReservationRepository(MongoDbSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _collection = database.GetCollection<EnergyReservation>("EnergyReservations");
        }

        // ============================================================
        // READ OPERATIONS
        // ============================================================

        /// <summary>Returns all reservation documents from MongoDB.</summary>
        public async Task<List<EnergyReservation>> GetAllAsync()
        {
            return await _collection.Find(_ => true).ToListAsync();
        }

        /// <summary>Returns a single reservation by MongoDB ObjectId string.</summary>
        public async Task<EnergyReservation?> GetByIdAsync(string id)
        {
            return await _collection.Find(r => r.Id == id).FirstOrDefaultAsync();
        }

        /// <summary>Returns all reservations for a specific Prosumer NIC (booking history).</summary>
        public async Task<List<EnergyReservation>> GetByProsumerNicAsync(string nic)
        {
            return await _collection
                .Find(r => r.ProsumerNic == nic)
                .SortByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        /// <summary>Returns all reservations linked to a specific Station ID.</summary>
        public async Task<List<EnergyReservation>> GetByStationIdAsync(string stationId)
        {
            return await _collection
                .Find(r => r.StationId == stationId)
                .SortByDescending(r => r.ReservationDate)
                .ToListAsync();
        }

        /// <summary>Returns all reservations filtered by exact status string.</summary>
        public async Task<List<EnergyReservation>> GetByStatusAsync(string status)
        {
            return await _collection
                .Find(r => r.Status == status)
                .SortByDescending(r => r.ReservationDate)
                .ToListAsync();
        }

        /// <summary>Returns all Pending reservations sorted oldest first (FIFO approval).</summary>
        public async Task<List<EnergyReservation>> GetPendingAsync()
        {
            return await _collection
                .Find(r => r.Status == ReservationStatus.Pending)
                .SortBy(r => r.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Flexible multi-filter search. Any null parameter is ignored.
        /// Supports filtering by NIC, StationId, Status, and date range.
        /// </summary>
        public async Task<List<EnergyReservation>> SearchAsync(
            string? nic,
            string? stationId,
            string? status,
            DateTime? from,
            DateTime? to)
        {
            // Build filter dynamically using FilterDefinitionBuilder
            var builder = Builders<EnergyReservation>.Filter;
            var filter = builder.Empty; // Start with no filter (matches all)

            if (!string.IsNullOrWhiteSpace(nic))
                filter &= builder.Eq(r => r.ProsumerNic, nic);

            if (!string.IsNullOrWhiteSpace(stationId))
                filter &= builder.Eq(r => r.StationId, stationId);

            if (!string.IsNullOrWhiteSpace(status))
                filter &= builder.Eq(r => r.Status, status);

            if (from.HasValue)
                filter &= builder.Gte(r => r.ReservationDate, from.Value);

            if (to.HasValue)
                filter &= builder.Lte(r => r.ReservationDate, to.Value.AddDays(1)); // inclusive end date

            return await _collection
                .Find(filter)
                .SortByDescending(r => r.ReservationDate)
                .ToListAsync();
        }

        /// <summary>
        /// Checks if a station has any active (Pending or Approved) reservations.
        /// Called by Member 2 via IReservationChecker to block station deactivation.
        /// </summary>
        public async Task<bool> HasActiveReservationsForStationAsync(string stationId)
        {
            var activeStatuses = new[] { ReservationStatus.Pending, ReservationStatus.Approved };
            var count = await _collection.CountDocumentsAsync(r =>
                r.StationId == stationId && activeStatuses.Contains(r.Status));
            return count > 0;
        }

        /// <summary>
        /// Checks if a slot already has a Pending or Approved reservation for the same date.
        /// Used during CreateReservation to prevent double-booking the same slot.
        /// </summary>
        public async Task<bool> HasConflictingReservationForSlotAsync(string slotId, DateTime reservationDate)
        {
            var activeStatuses = new[] { ReservationStatus.Pending, ReservationStatus.Approved };
            var date = reservationDate.Date; // Compare date only (ignore time component)
            var count = await _collection.CountDocumentsAsync(r =>
                r.SlotId == slotId &&
                r.ReservationDate >= date &&
                r.ReservationDate < date.AddDays(1) &&
                activeStatuses.Contains(r.Status));
            return count > 0;
        }

        /// <summary>Returns count of Approved reservations with a future date (for dashboard).</summary>
        public async Task<long> CountApprovedFutureAsync()
        {
            return await _collection.CountDocumentsAsync(r =>
                r.Status == ReservationStatus.Approved &&
                r.ReservationDate >= DateTime.UtcNow);
        }

        /// <summary>Returns count of all Pending reservations (for dashboard).</summary>
        public async Task<long> CountPendingAsync()
        {
            return await _collection.CountDocumentsAsync(r =>
                r.Status == ReservationStatus.Pending);
        }

        // ============================================================
        // WRITE OPERATIONS
        // ============================================================

        /// <summary>Inserts a new reservation document and returns it with the generated Id.</summary>
        public async Task<EnergyReservation> CreateAsync(EnergyReservation reservation)
        {
            await _collection.InsertOneAsync(reservation);
            return reservation;
        }

        /// <summary>Replaces an entire reservation document by its MongoDB _id.</summary>
        public async Task UpdateAsync(string id, EnergyReservation reservation)
        {
            await _collection.ReplaceOneAsync(r => r.Id == id, reservation);
        }

        /// <summary>Hard deletes a reservation document by its MongoDB _id.</summary>
        public async Task DeleteAsync(string id)
        {
            await _collection.DeleteOneAsync(r => r.Id == id);
        }
    }
}
