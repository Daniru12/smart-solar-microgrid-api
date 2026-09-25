using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSolarMicrogrid.API.Components.Identity.DTOs;
using SmartSolarMicrogrid.API.Components.Identity.Interfaces;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SmartSolarMicrogrid.API.Components.Identity.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        [Authorize(Roles = "Backoffice")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            try
            {
                var user = await _userService.CreateUserAsync(request);
                return Created("", new { Success = true, Data = user });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Backoffice")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(new { Success = true, Data = users });
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Backoffice")]
        public async Task<IActionResult> GetUserById(string id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                return Ok(new { Success = true, Data = user });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { Success = false, Message = "User not found." });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Backoffice")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserRequest request)
        {
            try
            {
                var user = await _userService.UpdateUserAsync(id, request);
                return Ok(new { Success = true, Data = user });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { Success = false, Message = "User not found." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }

        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Backoffice")]
        public async Task<IActionResult> UpdateStatus(string id, [FromBody] UpdateUserStatusRequest request)
        {
            try
            {
                await _userService.UpdateUserStatusAsync(id, request);
                return Ok(new { Success = true, Message = "Status updated successfully." });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { Success = false, Message = "User not found." });
            }
        }

        [HttpPost("{id}/reset-password")]
        [Authorize(Roles = "Backoffice")]
        public async Task<IActionResult> ResetPassword(string id, [FromBody] ResetPasswordRequest request)
        {
            try
            {
                await _userService.ResetPasswordAsync(id, request);
                return Ok(new { Success = true, Message = "Password reset successfully." });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { Success = false, Message = "User not found." });
            }
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { Success = false, Message = "Invalid token." });

                var user = await _userService.GetCurrentUserAsync(userId);
                return Ok(new { Success = true, Data = user });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { Success = false, Message = "User not found." });
            }
        }

        [HttpPost("me/change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { Success = false, Message = "Invalid token." });

                await _userService.ChangePasswordAsync(userId, request);
                return Ok(new { Success = true, Message = "Password changed successfully." });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { Success = false, Message = "User not found." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Success = false, Message = ex.Message });
            }
        }
    }
}
