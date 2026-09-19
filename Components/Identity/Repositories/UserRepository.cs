using MongoDB.Driver;
using SmartSolarMicrogrid.API.Components.Identity.Interfaces;
using SmartSolarMicrogrid.API.Components.Identity.Models;
using SmartSolarMicrogrid.API.Infrastructure.MongoDB;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartSolarMicrogrid.API.Components.Identity.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IMongoCollection<User> _users;

        public UserRepository(MongoDbSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _users = database.GetCollection<User>("Users");
        }

        public async Task<User> GetByIdAsync(string id) =>
            await _users.Find(u => u.Id == id).FirstOrDefaultAsync();

        public async Task<User> GetByEmailAsync(string email) =>
            await _users.Find(u => u.Email == email).FirstOrDefaultAsync();

        public async Task<List<User>> GetAllAsync() =>
            await _users.Find(_ => true).ToListAsync();

        public async Task CreateAsync(User user) =>
            await _users.InsertOneAsync(user);

        public async Task UpdateAsync(string id, User user) =>
            await _users.ReplaceOneAsync(u => u.Id == id, user);
    }
}
