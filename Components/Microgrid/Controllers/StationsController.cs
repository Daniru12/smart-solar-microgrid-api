using Microsoft.AspNetCore.Mvc;
using SmartSolarMicrogrid.API.Components.Microgrid.DTOs;
using SmartSolarMicrogrid.API.Components.Microgrid.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace SmartSolarMicrogrid.API.Components.Microgrid.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StationsController : ControllerBase
    {
        private readonly IMicrogridService _microgridService;

        public StationsController(IMicrogridService microgridService)
        {
            _microgridService = microgridService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllStations([FromQuery] string? status, [FromQuery] string? gridOperator)
        {
            bool activeOnly = status?.Equals("active", StringComparison.OrdinalIgnoreCase) ?? false;
            var stations = await _microgridService.GetAllStationsAsync(activeOnly);
            
            if (!string.IsNullOrWhiteSpace(gridOperator))
            {
                stations = stations.Where(s => 
                    s.GridOperatorName.Equals(gridOperator, StringComparison.OrdinalIgnoreCase) ||
                    s.StationId.Equals(gridOperator, StringComparison.OrdinalIgnoreCase));
            }

            return Ok(stations);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetStationById(string id)
        {
            var station = await _microgridService.GetStationByIdAsync(id);
            if (station == null) return NotFound("Station not found.");
            
            return Ok(station);
        }

        [HttpPost]
        [Authorize(Roles = "GridOperator,Admin")]
        public async Task<IActionResult> CreateStation([FromBody] CreateStationDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            if (userRole == "GridOperator" && !string.IsNullOrEmpty(userEmail))
            {
                dto.GridOperatorName = userEmail;
            }

            var created = await _microgridService.CreateStationAsync(dto);
            return CreatedAtAction(nameof(GetStationById), new { id = created.StationId }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStation(string id, [FromBody] UpdateStationDto dto)
        {
            var success = await _microgridService.UpdateStationAsync(id, dto);
            if (!success) return NotFound("Station not found.");
            
            return NoContent();
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStationStatus(string id, [FromBody] string status)
        {
            try
            {
                bool success;
                if (status.Equals("Active", StringComparison.OrdinalIgnoreCase))
                {
                    success = await _microgridService.ActivateStationAsync(id);
                }
                else if (status.Equals("Inactive", StringComparison.OrdinalIgnoreCase))
                {
                    success = await _microgridService.DeactivateStationAsync(id);
                }
                else
                {
                    return BadRequest("Invalid status. Use 'Active' or 'Inactive'.");
                }

                if (!success) return NotFound("Station not found.");
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
