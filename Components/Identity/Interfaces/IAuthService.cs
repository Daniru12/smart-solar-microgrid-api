using SmartSolarMicrogrid.API.Components.Identity.DTOs;
using System.Threading.Tasks;

namespace SmartSolarMicrogrid.API.Components.Identity.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(LoginRequest request);
    }
}
