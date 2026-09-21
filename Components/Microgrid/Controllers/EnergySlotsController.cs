using Microsoft.AspNetCore.Mvc;
using SmartSolarMicrogrid.API.Components.Microgrid.DTOs;
using SmartSolarMicrogrid.API.Components.Microgrid.Interfaces;

namespace SmartSolarMicrogrid.API.Components.Microgrid.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EnergySlotsController : ControllerBase
    {
        private readonly IMicrogridService _microgridService;

        public EnergySlotsController(IMicrogridService microgridService)
        {
            _microgridService = microgridService;
        }

        [HttpGet("~/api/stations/{stationId}/slots")]
        public async Task<IActionResult> GetSlotsForStation(string stationId, [FromQuery] string? date, [FromQuery] string? status)
        {
            DateTime? parsedDate = null;
            if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out var d))
            {
                parsedDate = d;
            }
            
            bool availableOnly = status?.Equals("available", StringComparison.OrdinalIgnoreCase) ?? false;
            
            var slots = await _microgridService.GetSlotsByStationAsync(stationId, parsedDate, availableOnly);
            return Ok(slots);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSlotById(string id)
        {
            var slot = await _microgridService.GetSlotByIdAsync(id);
            if (slot == null) return NotFound("Slot not found.");
            
            return Ok(slot);
        }

        [HttpPost("~/api/stations/{stationId}/slots")]
        public async Task<IActionResult> CreateSlot(string stationId, [FromBody] CreateSlotDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            try
            {
                var created = await _microgridService.CreateSlotAsync(stationId, dto);
                return CreatedAtAction(nameof(GetSlotById), new { id = created.SlotId }, created);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSlot(string id, [FromBody] UpdateSlotDto dto)
        {
            var success = await _microgridService.UpdateSlotAsync(id, dto);
            if (!success) return NotFound("Slot not found.");
            
            return NoContent();
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateSlotStatus(string id, [FromBody] string status)
        {
            if (string.IsNullOrEmpty(status)) return BadRequest("Status is required.");
            
            var success = await _microgridService.UpdateSlotAsync(id, new UpdateSlotDto { Status = status });
            if (!success) return NotFound("Slot not found.");
            
            return NoContent();
        }
    }
}
