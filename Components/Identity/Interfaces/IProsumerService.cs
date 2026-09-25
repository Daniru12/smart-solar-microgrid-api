using SmartSolarMicrogrid.API.Components.Identity.DTOs;
using System.Collections.Generic;
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
        Task<List<ProsumerProfileResponse>> GetAllProsumersAsync(ProsumerListRequest request);
        Task<ProsumerProfileResponse> GetProsumerByIdAsync(string id);
        Task<ProsumerProfileResponse> GetProsumerByNicAsync(string nic);
        Task<List<ProsumerProfileResponse>> GetDeactivationRequestsAsync();
        Task ApproveDeactivationAsync(string id);
        Task RejectDeactivationAsync(string id);
    }
}
