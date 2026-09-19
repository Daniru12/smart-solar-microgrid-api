using MongoDB.Driver;
using SmartSolarMicrogrid.API.Components.Identity.Interfaces;
using SmartSolarMicrogrid.API.Components.Identity.Models;
using SmartSolarMicrogrid.API.Infrastructure.MongoDB;
using System.Threading.Tasks;

namespace SmartSolarMicrogrid.API.Components.Identity.Repositories
{
    public class ProsumerRepository : IProsumerRepository
    {
        private readonly IMongoCollection<Prosumer> _prosumers;

        public ProsumerRepository(MongoDbSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _prosumers = database.GetCollection<Prosumer>("Prosumers");
        }

        public async Task<Prosumer> GetByIdAsync(string id) =>
            await _prosumers.Find(p => p.Id == id).FirstOrDefaultAsync();

        public async Task<Prosumer> GetByUserIdAsync(string userId) =>
            await _prosumers.Find(p => p.UserId == userId).FirstOrDefaultAsync();

        public async Task<Prosumer> GetByNicAsync(string nic) =>
            await _prosumers.Find(p => p.NIC == nic).FirstOrDefaultAsync();

        public async Task CreateAsync(Prosumer prosumer) =>
            await _prosumers.InsertOneAsync(prosumer);

        public async Task UpdateAsync(string id, Prosumer prosumer) =>
            await _prosumers.ReplaceOneAsync(p => p.Id == id, prosumer);
    }
}
