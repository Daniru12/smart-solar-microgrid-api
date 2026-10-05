using SmartSolarMicrogrid.API.Components.Identity.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartSolarMicrogrid.API.Components.Identity.Interfaces
{
    public interface IRoleTabPermissionsRepository
    {

        Task<RoleTabPermissions?> GetByRoleAsync(Role role);

        Task<List<RoleTabPermissions>> GetAllAsync();

        Task CreateOrUpdateAsync(RoleTabPermissions permissions);

        Task DeleteAsync(Role role);
    }
}