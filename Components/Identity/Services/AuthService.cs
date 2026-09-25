using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SmartSolarMicrogrid.API.Components.Identity.DTOs;
using SmartSolarMicrogrid.API.Components.Identity.Interfaces;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SmartSolarMicrogrid.API.Components.Identity.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IProsumerRepository _prosumerRepository;
        private readonly IConfiguration _configuration;

        public AuthService(IUserRepository userRepository, IProsumerRepository prosumerRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _prosumerRepository = prosumerRepository;
            _configuration = configuration;
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

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:SecretKey"] ?? "");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id ?? string.Empty),
                    new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                    new Claim(ClaimTypes.Role, user.Role.ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(2),
                Issuer = _configuration["JwtSettings:Issuer"],
                Audience = _configuration["JwtSettings:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            
            var token = tokenHandler.CreateToken(tokenDescriptor);

            string? nic = null;
            string? name = null;

            if (user.Role == Models.Role.Prosumer)
            {
                var prosumer = await _prosumerRepository.GetByUserIdAsync(user.Id ?? "");
                if (prosumer != null)
                {
                    nic = prosumer.NIC;
                    name = $"{prosumer.FirstName} {prosumer.LastName}".Trim();
                }
            }
            else if (user.Role == Models.Role.GridOperator)
            {
                name = "Grid Operator";
            }

            return new LoginResponse
            {
                Token = tokenHandler.WriteToken(token),
                UserId = user.Id ?? string.Empty,
                Role = user.Role.ToString(),
                Email = user.Email,
                Nic = nic,
                Name = name
            };
        }
    }
}
