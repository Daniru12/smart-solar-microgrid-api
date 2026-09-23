using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSolarMicrogrid.API.Components.Identity.DTOs;
using SmartSolarMicrogrid.API.Components.Identity.Interfaces;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SmartSolarMicrogrid.API.Components.Identity.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProsumersController : ControllerBase
    {
        private readonly IProsumerService _prosumerService;

        public ProsumersController(IProsumerService prosumerService)
        {
            _prosumerService = prosumerService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterProsumer([FromBody] RegisterProsumerRequest request)
        {
            try
            {
                var prosumer = await _prosumerService.RegisterProsumerAsync(request);
                return Created("", new { Success = true, Data = prosumer });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Backoffice")]
        public async Task<IActionResult> GetAllProsumers([FromQuery] ProsumerListRequest request)
        {
            try
            {
                var prosumers = await _prosumerService.GetAllProsumersAsync(request);
                return Ok(new { Success = true, Data = prosumers });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Backoffice")]
        public async Task<IActionResult> GetProsumerById(string id)
        {
            try
            {
                var prosumer = await _prosumerService.GetProsumerByIdAsync(id);
                return Ok(new { Success = true, Data = prosumer });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Success = false, Message = ex.Message });
            }
        }

        [HttpGet("nic/{nic}")]
        [Authorize(Roles = "Backoffice")]
        public async Task<IActionResult> GetProsumerByNic(string nic)
        {
            try
            {
                var prosumer = await _prosumerService.GetProsumerByNicAsync(nic);
                return Ok(new { Success = true, Data = prosumer });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Success = false, Message = ex.Message });
            }
        }

        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Backoffice")]
        public async Task<IActionResult> UpdateStatus(string id, [FromBody] UpdateUserStatusRequest request)
        {
            try
            {
                await _prosumerService.UpdateProsumerStatusAsync(id, request);
                return Ok(new { Success = true, Message = "Prosumer status updated successfully." });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { Success = false, Message = "Prosumer not found." });
            }
        }

        [HttpGet("me")]
        [Authorize(Roles = "Prosumer")]
        public async Task<IActionResult> GetCurrentProsumer()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { Success = false, Message = "Invalid token." });

                var profile = await _prosumerService.GetCurrentProsumerAsync(userId);
                return Ok(new { Success = true, Data = profile });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Success = false, Message = ex.Message });
            }
        }

        [HttpPut("me")]
        [Authorize(Roles = "Prosumer")]
        public async Task<IActionResult> UpdateCurrentProsumer([FromBody] UpdateProsumerRequest request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { Success = false, Message = "Invalid token." });

                var profile = await _prosumerService.UpdateCurrentProsumerAsync(userId, request);
                return Ok(new { Success = true, Data = profile });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Success = false, Message = ex.Message });
            }
        }

        [HttpPost("me/request-deactivation")]
        [Authorize(Roles = "Prosumer")]
        public async Task<IActionResult> RequestDeactivation()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { Success = false, Message = "Invalid token." });

                await _prosumerService.RequestDeactivationAsync(userId);
                return Ok(new { Success = true, Message = "Deactivation request submitted successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Success = false, Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }

        [HttpGet("deactivation-requests")]
        [Authorize(Roles = "Backoffice")]
        public async Task<IActionResult> GetDeactivationRequests()
        {
            try
            {
                var requests = await _prosumerService.GetDeactivationRequestsAsync();
                return Ok(new { Success = true, Data = requests });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }

        [HttpPost("{id}/approve-deactivation")]
        [Authorize(Roles = "Backoffice")]
        public async Task<IActionResult> ApproveDeactivation(string id)
        {
            try
            {
                await _prosumerService.ApproveDeactivationAsync(id);
                return Ok(new { Success = true, Message = "Deactivation approved successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Success = false, Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }

        [HttpPost("{id}/reject-deactivation")]
        [Authorize(Roles = "Backoffice")]
        public async Task<IActionResult> RejectDeactivation(string id)
        {
            try
            {
                await _prosumerService.RejectDeactivationAsync(id);
                return Ok(new { Success = true, Message = "Deactivation rejected successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Success = false, Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }

        [HttpPost("me/change-password")]
        [Authorize(Roles = "Prosumer")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { Success = false, Message = "Invalid token." });

                await _prosumerService.ChangePasswordAsync(userId, request);
                return Ok(new { Success = true, Message = "Password changed successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Success = false, Message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Success = false, Message = ex.Message });
            }
        }
    }
}
