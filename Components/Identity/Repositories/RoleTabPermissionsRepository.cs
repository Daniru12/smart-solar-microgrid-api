using MongoDB.Driver;
using SmartSolarMicrogrid.API.Components.Identity.Interfaces;
using SmartSolarMicrogrid.API.Components.Identity.Models;
using SmartSolarMicrogrid.API.Infrastructure.MongoDB;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartSolarMicrogrid.API.Components.Identity.Repositories
{
    public class RoleTabPermissionsRepository : IRoleTabPermissionsRepository
    {
        private readonly IMongoCollection<RoleTabPermissions> _roleTabPermissions;

        public RoleTabPermissionsRepository(MongoDbSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _roleTabPermissions = database.GetCollection<RoleTabPermissions>("RoleTabPermissions");

            // Create index on Role field for efficient lookups
            var indexKeysDefinition = Builders<RoleTabPermissions>.IndexKeys.Ascending(x => x.Role);
            var indexModel = new CreateIndexModel<RoleTabPermissions>(indexKeysDefinition);
            _roleTabPermissions.Indexes.CreateOneAsync(indexModel);
        }

        public async Task<RoleTabPermissions?> GetByRoleAsync(Role role) =>
            await _roleTabPermissions.Find(x => x.Role == role).FirstOrDefaultAsync();

        public async Task<List<RoleTabPermissions>> GetAllAsync() =>
            await _roleTabPermissions.Find(_ => true).ToListAsync();

        public async Task CreateOrUpdateAsync(RoleTabPermissions permissions)
        {
            var existing = await GetByRoleAsync(permissions.Role);
            if (existing != null)
            {
                // Update existing - preserve the Id
                permissions.Id = existing.Id;
                permissions.UpdatedAt = System.DateTime.UtcNow;
                await _roleTabPermissions.ReplaceOneAsync(x => x.Role == permissions.Role, permissions);
            }
            else
            {
                // Create new
                await _roleTabPermissions.InsertOneAsync(permissions);
            }
        }

        public async Task DeleteAsync(Role role) =>
            await _roleTabPermissions.DeleteOneAsync(x => x.Role == role);
    }
}