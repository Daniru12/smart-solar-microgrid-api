using SmartSolarMicrogrid.API.Components.Identity.DTOs;
using SmartSolarMicrogrid.API.Components.Identity.Interfaces;
using System;
using System.Threading.Tasks;

namespace SmartSolarMicrogrid.API.Components.Identity.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;

        public AuthService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null || user.Status != Models.AccountStatus.Active)
            {
                throw new UnauthorizedAccessException("Invalid credentials or inactive account.");
            }

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Invalid credentials.");
            }

            // Return a simple dummy token for the initial assignment phase
            return new LoginResponse
            {
                Token = $"dummy-token-for-{user.Id}",
                UserId = user.Id,
                Role = user.Role.ToString()
            };
        }
    }
}
