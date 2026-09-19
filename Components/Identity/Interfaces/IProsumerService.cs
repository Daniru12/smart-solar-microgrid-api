using SmartSolarMicrogrid.API.Components.Identity.DTOs;
using System.Threading.Tasks;

namespace SmartSolarMicrogrid.API.Components.Identity.Interfaces
{
    public interface IProsumerService
    {
        Task<ProsumerDto> RegisterProsumerAsync(RegisterProsumerRequest request);
        Task UpdateProsumerStatusAsync(string id, UpdateUserStatusRequest request);
    }
}
