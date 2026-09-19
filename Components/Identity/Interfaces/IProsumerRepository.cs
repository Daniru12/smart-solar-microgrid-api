using SmartSolarMicrogrid.API.Components.Identity.Models;
using System.Threading.Tasks;

namespace SmartSolarMicrogrid.API.Components.Identity.Interfaces
{
    public interface IProsumerRepository
    {
        Task<Prosumer> GetByIdAsync(string id);
        Task<Prosumer> GetByUserIdAsync(string userId);
        Task<Prosumer> GetByNicAsync(string nic);
        Task CreateAsync(Prosumer prosumer);
        Task UpdateAsync(string id, Prosumer prosumer);
    }
}
