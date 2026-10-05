using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSolarMicrogrid.API.Components.Identity.DTOs;
using SmartSolarMicrogrid.API.Components.Identity.Interfaces;
using SmartSolarMicrogrid.API.Components.Identity.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartSolarMicrogrid.API.Components.Identity.Controllers
{
    [ApiController]
    [Route("api/roles")]
    public class RoleTabPermissionsController : ControllerBase
    {
        private readonly IRoleTabPermissionsService _roleTabPermissionsService;

        public RoleTabPermissionsController(IRoleTabPermissionsService roleTabPermissionsService)
        {
            _roleTabPermissionsService = roleTabPermissionsService;
        }

        // ── Role Tab Permissions ──────────────────────────────────────────────────────

        /// <summary>
        /// Gets the sidebar tab permissions for a specific role.
        /// Only Backoffice users can call this.
        /// </summary>
        [HttpGet("{role}/tabs")]
        [Authorize]
        public async Task<IActionResult> GetTabPermissionsByRole(string role)
        {
            try
            {
                Console.WriteLine($"GET: Received role string: '{role}'");

                if (!Enum.TryParse<Role>(role, true, out var roleEnum))
                {
                    Console.WriteLine($"GET: Failed to parse role: '{role}'");
                    return BadRequest(new { Success = false, Message = "Invalid role." });
                }

                Console.WriteLine($"GET: Successfully parsed role to: {roleEnum}");

                var result = await _roleTabPermissionsService.GetTabPermissionsByRoleAsync(roleEnum);
                Console.WriteLine($"GET: Result - Role: {result.Role}, VisibleTabs: {System.Text.Json.JsonSerializer.Serialize(result.VisibleTabs)}");

                return Ok(new { Success = true, Data = result });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GET: Exception: {ex.Message}");
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }

        /// <summary>
        /// Updates the sidebar tab permissions for a specific role.
        /// Pass null visibleTabs to restore default (all tabs visible).
        /// Only Backoffice users can call this.
        /// </summary>
        [HttpPut("{role}/tabs")]
        [Authorize(Roles = "Backoffice")]
        public async Task<IActionResult> UpdateTabPermissionsByRole(string role, [FromBody] UpdateRoleTabPermissionsRequest request)
        {
            try
            {
                Console.WriteLine($"Received role string: '{role}'");
                Console.WriteLine($"Request visibleTabs: {System.Text.Json.JsonSerializer.Serialize(request.VisibleTabs)}");

                if (!Enum.TryParse<Role>(role, true, out var roleEnum))
                {
                    Console.WriteLine($"Failed to parse role: '{role}'");
                    return BadRequest(new { Success = false, Message = $"Invalid role: {role}" });
                }

                Console.WriteLine($"Successfully parsed role to: {roleEnum}");

                // Prevent configuration of Backoffice role - they always see everything
                if (roleEnum == Role.Backoffice)
                {
                    return BadRequest(new { Success = false, Message = "Backoffice role cannot be configured. Backoffice users always see all tabs." });
                }

                var result = await _roleTabPermissionsService.UpdateTabPermissionsByRoleAsync(roleEnum, request);
                return Ok(new { Success = true, Data = result, Message = "Tab permissions updated successfully." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }

        /// <summary>
        /// Gets tab permissions for all roles.
        /// Only Backoffice users can call this.
        /// </summary>
        [HttpGet("tabs")]
        [Authorize(Roles = "Backoffice")]
        public async Task<IActionResult> GetAllRolePermissions()
        {
            try
            {
                var result = await _roleTabPermissionsService.GetAllRolePermissionsAsync();
                return Ok(new { Success = true, Data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }
    }
}