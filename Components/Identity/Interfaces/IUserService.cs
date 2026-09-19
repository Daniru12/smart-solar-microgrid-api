using SmartSolarMicrogrid.API.Components.Identity.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartSolarMicrogrid.API.Components.Identity.Interfaces
{
    public interface IUserService
    {
        Task<UserDto> CreateUserAsync(CreateUserRequest request);
        Task<List<UserDto>> GetAllUsersAsync();
        Task UpdateUserStatusAsync(string id, UpdateUserStatusRequest request);
    }
}
