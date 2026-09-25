/*
 * File: EnergyTransferRepository.cs
 * Description: MongoDB access for EnergyTransfers. One document per reservation.
 */

using MongoDB.Driver;
using SmartSolarMicrogrid.API.Components.Transaction.Interfaces;
using SmartSolarMicrogrid.API.Components.Transaction.Models;
using SmartSolarMicrogrid.API.Infrastructure.MongoDB;

namespace SmartSolarMicrogrid.API.Components.Transaction.Repositories
{
    public class EnergyTransferRepository : IEnergyTransferRepository
    {
        private readonly IMongoCollection<EnergyTransfer> _transfers;

        public EnergyTransferRepository(MongoDbSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _transfers = database.GetCollection<EnergyTransfer>("EnergyTransfers");
        }

        public async Task<EnergyTransfer?> GetByReservationIdAsync(string reservationId)
        {
            return await _transfers.Find(t => t.ReservationId == reservationId).FirstOrDefaultAsync();
        }

        public async Task<EnergyTransfer> CreateAsync(EnergyTransfer transfer)
        {
            await _transfers.InsertOneAsync(transfer);
            return transfer;
        }

        public async Task<bool> UpdateAsync(EnergyTransfer transfer)
        {
            var result = await _transfers.ReplaceOneAsync(t => t.ReservationId == transfer.ReservationId, transfer);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }
    }
}
