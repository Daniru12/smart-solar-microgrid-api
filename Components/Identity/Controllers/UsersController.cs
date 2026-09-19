using Microsoft.AspNetCore.Mvc;
using SmartSolarMicrogrid.API.Components.Identity.DTOs;
using SmartSolarMicrogrid.API.Components.Identity.Interfaces;
using System;
using System.Collections.Generic;
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
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(new { Success = true, Data = users });
        }

        [HttpPatch("{id}/status")]
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
    }
}
