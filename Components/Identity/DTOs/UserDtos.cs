using SmartSolarMicrogrid.API.Components.Identity.Models;
using System;
using System.Collections.Generic;

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
        public string Status { get; set; } = string.Empty;
    }

    public class UpdateUserRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class ResetPasswordRequest
    {
        public string NewPassword { get; set; } = string.Empty;
    }

    public class UserProfileResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class UpdateRoleTabPermissionsRequest
    {

        public List<string>? VisibleTabs { get; set; }
    }

    public class RoleTabPermissionsResponse
    {
        public string Role { get; set; } = string.Empty;

        public List<string>? VisibleTabs { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
