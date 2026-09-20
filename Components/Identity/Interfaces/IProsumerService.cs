using SmartSolarMicrogrid.API.Components.Identity.DTOs;
using System.Threading.Tasks;

namespace SmartSolarMicrogrid.API.Components.Identity.Interfaces
{
    public interface IProsumerService
    {
        Task<ProsumerDto> RegisterProsumerAsync(RegisterProsumerRequest request);
        Task UpdateProsumerStatusAsync(string id, UpdateUserStatusRequest request);
        Task<ProsumerProfileResponse> GetCurrentProsumerAsync(string userId);
        Task<ProsumerProfileResponse> UpdateCurrentProsumerAsync(string userId, UpdateProsumerRequest request);
        Task RequestDeactivationAsync(string userId);
        Task ChangePasswordAsync(string userId, ChangePasswordRequest request);
    }
}
