

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSolarMicrogrid.API.Components.Reservations.DTOs;
using SmartSolarMicrogrid.API.Components.Reservations.Interfaces;

namespace SmartSolarMicrogrid.API.Components.Reservations.Controllers
{

    [ApiController]
    [Route("api/reservations")]
    [Authorize]
    public class ReservationController : ControllerBase
    {
        private readonly IReservationService _service;

        public ReservationController(IReservationService service)
        {
            _service = service;
        }

        private string? GetOperatorStationId()
        {
            if (User.IsInRole("GridOperator"))
            {
                return User.FindFirst("StationId")?.Value;
            }
            return null;
        }

        [HttpGet]
        [Authorize(Roles = "Backoffice,GridOperator")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync(GetOperatorStationId());
            return Ok(new { Success = true, Data = result });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                var result = await _service.GetByIdAsync(id);
                return Ok(new { Success = true, Data = result });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Success = false, Message = ex.Message });
            }
        }

        [HttpGet("pending")]
        [Authorize(Roles = "Backoffice,GridOperator")]
        public async Task<IActionResult> GetPending()
        {
            var result = await _service.GetPendingAsync(GetOperatorStationId());
            return Ok(new { Success = true, Data = result });
        }

        [HttpGet("prosumer/{nic}")]
        public async Task<IActionResult> GetByProsumerNic(string nic)
        {
            var result = await _service.GetByProsumerNicAsync(nic);
            return Ok(new { Success = true, Data = result });
        }

        [HttpGet("search")]
        [Authorize(Roles = "Backoffice,GridOperator")]
        public async Task<IActionResult> Search(
            [FromQuery] string? nic,
            [FromQuery] string? stationId,
            [FromQuery] string? status,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            var searchDto = new ReservationSearchDto
            {
                Nic = nic,
                StationId = stationId,
                Status = status,
                From = from,
                To = to
            };
            var result = await _service.SearchAsync(searchDto, GetOperatorStationId());
            return Ok(new { Success = true, Data = result });
        }

        [HttpGet("prosumer/{nic}/search")]
        [Authorize(Roles = "Prosumer,Backoffice,GridOperator")]
        public async Task<IActionResult> SearchByProsumer(
            string nic,
            [FromQuery] string? status,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            var searchDto = new ReservationSearchDto
            {
                Nic = nic,
                Status = status,
                From = from,
                To = to
            };
            var result = await _service.SearchAsync(searchDto);
            return Ok(new { Success = true, Data = result });
        }

        [HttpGet("dashboard")]
        [Authorize(Roles = "Backoffice,GridOperator")]
        public async Task<IActionResult> GetDashboardSummary()
        {
            var result = await _service.GetDashboardSummaryAsync(GetOperatorStationId());
            return Ok(new { Success = true, Data = result });
        }

        [HttpPost]
        [Authorize(Roles = "Prosumer")]
        public async Task<IActionResult> Create([FromBody] CreateReservationDto dto)
        {
            try
            {
                var result = await _service.CreateReservationAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = result.Id },
                    new { Success = true, Data = result, Message = "Reservation created successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Prosumer")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateReservationDto dto)
        {
            try
            {
                var result = await _service.UpdateReservationAsync(id, dto);
                return Ok(new { Success = true, Data = result, Message = "Reservation updated successfully." });
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

        [HttpPut("{id}/approve")]
        [Authorize(Roles = "Backoffice,GridOperator")]
        public async Task<IActionResult> Approve(string id)
        {
            try
            {
                var result = await _service.ApproveReservationAsync(id, GetOperatorStationId());
                return Ok(new { Success = true, Data = result, Message = "Reservation approved successfully." });
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

        [HttpGet("{id}/validate-qr")]
        [Authorize(Roles = "GridOperator,Backoffice")]
        public async Task<IActionResult> ValidateQr(string id)
        {
            try
            {
                var result = await _service.ValidateQrAsync(id, GetOperatorStationId());
                return Ok(new { Success = true, Data = result, Message = "QR is valid and reservation is approved." });
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

        [HttpPut("{id}/complete")]
        [Authorize(Roles = "GridOperator,Backoffice")]
        public async Task<IActionResult> Complete(string id)
        {
            try
            {
                var result = await _service.CompleteReservationAsync(id, GetOperatorStationId());
                return Ok(new { Success = true, Data = result, Message = "Energy transfer confirmed and reservation completed." });
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

        [HttpPut("{id}/cancel")]
        [Authorize(Roles = "Prosumer,Backoffice,GridOperator")]
        public async Task<IActionResult> Cancel(string id)
        {
            try
            {
                string? operatorStationId = User.IsInRole("GridOperator") ? GetOperatorStationId() : null;
                var result = await _service.CancelReservationAsync(id, operatorStationId);
                return Ok(new { Success = true, Data = result, Message = "Reservation cancelled successfully." });
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

        [HttpDelete("{id}")]
        [Authorize(Roles = "Backoffice")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                await _service.DeleteReservationAsync(id);
                return Ok(new { Success = true, Message = "Reservation deleted successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Success = false, Message = ex.Message });
            }
        }
    }
}

