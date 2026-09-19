using SmartSolarMicrogrid.API.Components.Identity.DTOs;
using SmartSolarMicrogrid.API.Components.Identity.Interfaces;
using SmartSolarMicrogrid.API.Components.Identity.Models;
using System;
using System.Threading.Tasks;

namespace SmartSolarMicrogrid.API.Components.Identity.Services
{
    public class ProsumerService : IProsumerService
    {
        private readonly IProsumerRepository _prosumerRepository;
        private readonly IUserRepository _userRepository;

        public ProsumerService(IProsumerRepository prosumerRepository, IUserRepository userRepository)
        {
            _prosumerRepository = prosumerRepository;
            _userRepository = userRepository;
        }

        public async Task<ProsumerDto> RegisterProsumerAsync(RegisterProsumerRequest request)
        {
            var existingUser = await _userRepository.GetByEmailAsync(request.Email);
            if (existingUser != null) throw new InvalidOperationException("Email already exists.");

            var existingNic = await _prosumerRepository.GetByNicAsync(request.NIC);
            if (existingNic != null) throw new InvalidOperationException("NIC already registered.");

            var user = new User
            {
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = Role.Prosumer,
                Status = AccountStatus.Pending // Prosumers might need activation
            };
            await _userRepository.CreateAsync(user);

            var prosumer = new Prosumer
            {
                UserId = user.Id,
                NIC = request.NIC,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                Address = request.Address
            };
            await _prosumerRepository.CreateAsync(prosumer);

            return new ProsumerDto
            {
                Id = prosumer.Id,
                UserId = prosumer.UserId,
                NIC = prosumer.NIC,
                FirstName = prosumer.FirstName,
                LastName = prosumer.LastName,
                Email = user.Email,
                Status = user.Status.ToString()
            };
        }

        public async Task UpdateProsumerStatusAsync(string id, UpdateUserStatusRequest request)
        {
            var prosumer = await _prosumerRepository.GetByIdAsync(id);
            if (prosumer == null) throw new KeyNotFoundException("Prosumer not found.");

            var user = await _userRepository.GetByIdAsync(prosumer.UserId);
            if (user != null)
            {
                user.Status = request.Status;
                await _userRepository.UpdateAsync(user.Id, user);
            }
        }
    }
}
