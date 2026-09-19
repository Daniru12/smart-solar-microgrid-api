using SmartSolarMicrogrid.API.Components.Identity.Models;
using System;

namespace SmartSolarMicrogrid.API.Components.Identity.DTOs
{
    public class UserDto
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class CreateUserRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public Role Role { get; set; }
    }

    public class UpdateUserStatusRequest
    {
        public AccountStatus Status { get; set; }
    }
}
