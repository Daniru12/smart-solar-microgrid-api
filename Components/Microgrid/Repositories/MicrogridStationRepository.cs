using MongoDB.Driver;
using SmartSolarMicrogrid.API.Components.Microgrid.Interfaces;
using SmartSolarMicrogrid.API.Components.Microgrid.Models;
using SmartSolarMicrogrid.API.Infrastructure.MongoDB;

namespace SmartSolarMicrogrid.API.Components.Microgrid.Repositories
{
    public class MicrogridStationRepository : IMicrogridStationRepository
    {
        private readonly IMongoCollection<SolarStationInfo> _stations;

        public MicrogridStationRepository(MongoDbSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _stations = database.GetCollection<SolarStationInfo>("SolarStationInfo");
        }

        public async Task<IEnumerable<SolarStationInfo>> GetAllStationsAsync(bool activeOnly = false)
        {
            if (activeOnly)
            {
                return await _stations.Find(s => s.Status == "Active").ToListAsync();
            }
            return await _stations.Find(_ => true).ToListAsync();
        }

        public async Task<SolarStationInfo?> GetStationByIdAsync(string stationId)
        {
            return await _stations.Find(s => s.StationId == stationId).FirstOrDefaultAsync();
        }

        public async Task<SolarStationInfo> CreateStationAsync(SolarStationInfo station)
        {
            await _stations.InsertOneAsync(station);
            return station;
        }

        public async Task<bool> UpdateStationAsync(string stationId, SolarStationInfo stationIn)
        {
            var result = await _stations.ReplaceOneAsync(s => s.StationId == stationId, stationIn);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> UpdateStationStatusAsync(string stationId, string status)
        {
            var update = Builders<SolarStationInfo>.Update
                .Set(s => s.Status, status)
                .Set(s => s.UpdatedAt, DateTime.UtcNow);
            
            var result = await _stations.UpdateOneAsync(s => s.StationId == stationId, update);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }
    }
}
