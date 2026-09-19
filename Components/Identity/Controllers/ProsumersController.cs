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

        [HttpPatch("{id}/status")]
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
    }
}
