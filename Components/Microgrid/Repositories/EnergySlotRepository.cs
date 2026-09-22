using MongoDB.Driver;
using SmartSolarMicrogrid.API.Components.Microgrid.Interfaces;
using SmartSolarMicrogrid.API.Components.Microgrid.Models;
using SmartSolarMicrogrid.API.Infrastructure.MongoDB;

namespace SmartSolarMicrogrid.API.Components.Microgrid.Repositories
{
    public class EnergySlotRepository : IEnergySlotRepository
    {
        private readonly IMongoCollection<EnergyBookingSlot> _slots;

        public EnergySlotRepository(MongoDbSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _slots = database.GetCollection<EnergyBookingSlot>("EnergyBookingSlots");
        }

        public async Task<IEnumerable<EnergyBookingSlot>> GetSlotsByStationAsync(string stationId, DateTime? date = null, bool availableOnly = false)
        {
            var builder = Builders<EnergyBookingSlot>.Filter;
            var filter = builder.Eq(s => s.StationId, stationId);
            
            if (date.HasValue)
            {
                filter &= builder.Eq(s => s.Date, date.Value.Date);
            }
            
            if (availableOnly)
            {
                filter &= builder.Eq(s => s.Status, "Available");
            }
            
            return await _slots.Find(filter).ToListAsync();
        }

        public async Task<EnergyBookingSlot?> GetSlotByIdAsync(string slotId)
        {
            return await _slots.Find(s => s.SlotId == slotId).FirstOrDefaultAsync();
        }

        public async Task<EnergyBookingSlot> CreateSlotAsync(EnergyBookingSlot slot)
        {
            await _slots.InsertOneAsync(slot);
            return slot;
        }

        public async Task<bool> UpdateSlotAsync(string slotId, EnergyBookingSlot slotIn)
        {
            var result = await _slots.ReplaceOneAsync(s => s.SlotId == slotId, slotIn);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> UpdateSlotStatusAsync(string slotId, string status)
        {
            var update = Builders<EnergyBookingSlot>.Update
                .Set(s => s.Status, status)
                .Set(s => s.UpdatedAt, DateTime.UtcNow);
                
            var result = await _slots.UpdateOneAsync(s => s.SlotId == slotId, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<IEnumerable<EnergyBookingSlot>> GetSlotsByStationAndDateAsync(string stationId, DateTime date)
        {
            var builder = Builders<EnergyBookingSlot>.Filter;
            var filter = builder.Eq(s => s.StationId, stationId) & builder.Eq(s => s.Date, date.Date);
            
            return await _slots.Find(filter).ToListAsync();
        }
    }
}
