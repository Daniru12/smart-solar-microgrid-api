using SmartSolarMicrogrid.API.Components.Identity.DTOs;
using SmartSolarMicrogrid.API.Components.Identity.Interfaces;
using SmartSolarMicrogrid.API.Components.Identity.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartSolarMicrogrid.API.Components.Identity.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<UserDto> CreateUserAsync(CreateUserRequest request)
        {
            var existing = await _userRepository.GetByEmailAsync(request.Email);
            if (existing != null) throw new InvalidOperationException("Email already exists.");

            var user = new User
            {
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = request.Role,
                Status = AccountStatus.Active
            };

            await _userRepository.CreateAsync(user);

            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Role = user.Role.ToString(),
                Status = user.Status.ToString(),
                CreatedAt = user.CreatedAt
            };
        }

        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return users.Select(u => new UserDto
            {
                Id = u.Id,
                Email = u.Email,
                Role = u.Role.ToString(),
                Status = u.Status.ToString(),
                CreatedAt = u.CreatedAt
            }).ToList();
        }

        public async Task UpdateUserStatusAsync(string id, UpdateUserStatusRequest request)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) throw new KeyNotFoundException("User not found.");

            user.Status = request.Status;
            await _userRepository.UpdateAsync(id, user);
        }
    }
}
