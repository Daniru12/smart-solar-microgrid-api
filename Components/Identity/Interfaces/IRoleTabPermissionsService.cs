using SmartSolarMicrogrid.API.Components.Identity.DTOs;
using SmartSolarMicrogrid.API.Components.Identity.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartSolarMicrogrid.API.Components.Identity.Interfaces
{
    public interface IRoleTabPermissionsService
    {
        /// <summary>
        /// Gets tab permissions configuration for a specific role.
        /// Returns null VisibleTabs if the role has no configuration (indicating all tabs visible).
        /// </summary>
        Task<RoleTabPermissionsResponse> GetTabPermissionsByRoleAsync(Role role);

        /// <summary>
        /// Gets tab permissions configuration for all roles.
        /// </summary>
        Task<List<RoleTabPermissionsResponse>> GetAllRolePermissionsAsync();

        /// <summary>
        /// Updates tab permissions configuration for a specific role.
        /// Pass null VisibleTabs to reset the role to default (all tabs visible).
        /// </summary>
        Task<RoleTabPermissionsResponse> UpdateTabPermissionsByRoleAsync(Role role, UpdateRoleTabPermissionsRequest request);
    }
}