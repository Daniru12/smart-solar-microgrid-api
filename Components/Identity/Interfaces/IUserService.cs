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
        Task<UserProfileResponse> GetUserByIdAsync(string id);
        Task<UserProfileResponse> UpdateUserAsync(string id, UpdateUserRequest request);
        Task<UserProfileResponse> GetCurrentUserAsync(string userId);
        Task ChangePasswordAsync(string userId, ChangePasswordRequest request);
        Task ResetPasswordAsync(string id, ResetPasswordRequest request);
    }
}
