using SmartSolarMicrogrid.API.Components.Identity.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartSolarMicrogrid.API.Components.Identity.Interfaces
{
    public interface IRoleTabPermissionsRepository
    {
        /// <summary>
        /// Gets tab permissions configuration for a specific role.
        /// Returns null if the role has no configuration.
        /// </summary>
        Task<RoleTabPermissions?> GetByRoleAsync(Role role);

        /// <summary>
        /// Gets all role tab permissions configurations.
        /// </summary>
        Task<List<RoleTabPermissions>> GetAllAsync();

        /// <summary>
        /// Creates or updates tab permissions configuration for a role.
        /// </summary>
        Task CreateOrUpdateAsync(RoleTabPermissions permissions);

        /// <summary>
        /// Deletes tab permissions configuration for a role.
        /// </summary>
        Task DeleteAsync(Role role);
    }
}