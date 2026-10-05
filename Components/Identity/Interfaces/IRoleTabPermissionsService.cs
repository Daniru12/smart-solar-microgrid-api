using SmartSolarMicrogrid.API.Components.Identity.DTOs;
using SmartSolarMicrogrid.API.Components.Identity.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartSolarMicrogrid.API.Components.Identity.Interfaces
{
    public interface IRoleTabPermissionsService
    {

        Task<RoleTabPermissionsResponse> GetTabPermissionsByRoleAsync(Role role);

        Task<List<RoleTabPermissionsResponse>> GetAllRolePermissionsAsync();

        Task<RoleTabPermissionsResponse> UpdateTabPermissionsByRoleAsync(Role role, UpdateRoleTabPermissionsRequest request);
    }
}