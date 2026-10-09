

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSolarMicrogrid.API.Components.Transaction.DTOs;
using SmartSolarMicrogrid.API.Components.Transaction.Interfaces;
using System.Security.Claims;

namespace SmartSolarMicrogrid.API.Components.Transaction.Controllers
{
    [ApiController]
    [Route("api/transfers")]
    [Authorize]
    public class TransfersController : ControllerBase
    {
        private readonly IEnergyTransferService _service;

        public TransfersController(IEnergyTransferService service)
        {
            _service = service;
        }

        [HttpPost("reservations/{reservationId}/issue")]
        [Authorize(Roles = "Backoffice,GridOperator")]
        public async Task<IActionResult> IssueQr(string reservationId)
        {
            try
            {
                var result = await _service.IssueQrAsync(reservationId);
                return Ok(new { Success = true, Data = result, Message = result.Message });
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

        [HttpPut("reservations/{reservationId}/regenerate")]
        [Authorize(Roles = "Backoffice,GridOperator")]
        public async Task<IActionResult> RegenerateQr(string reservationId)
        {
            try
            {
                var result = await _service.RegenerateQrAsync(reservationId);
                return Ok(new { Success = true, Data = result, Message = result.Message });
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

        [HttpPut("reservations/{reservationId}")]
        [Authorize(Roles = "Backoffice,GridOperator")]
        public async Task<IActionResult> UpdateQr(string reservationId, [FromBody] UpdateTransferRequest request)
        {
            try
            {
                var result = await _service.UpdateQrAsync(reservationId, request);
                return Ok(new { Success = true, Data = result, Message = result.Message });
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

        [HttpDelete("reservations/{reservationId}")]
        [Authorize(Roles = "Backoffice,GridOperator")]
        public async Task<IActionResult> DeleteQr(string reservationId)
        {
            try
            {
                await _service.DeleteQrAsync(reservationId);
                return Ok(new { Success = true, Message = "QR deleted. The old code no longer works." });
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

        [HttpGet("reservations/{reservationId}")]
        public async Task<IActionResult> GetConfirmation(string reservationId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var role = User.FindFirstValue(ClaimTypes.Role);
                var result = await _service.GetConfirmationAsync(reservationId, userId, role);
                return Ok(new { Success = true, Data = result, Message = result.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Success = false, Message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { Success = false, Message = ex.Message });
            }
        }

        [HttpPost("verify")]
        [Authorize(Roles = "Backoffice,GridOperator")]
        public async Task<IActionResult> Verify([FromBody] VerifyQrRequest request)
        {
            try
            {
                var result = await _service.VerifyScanAsync(request.QrPayload);
                return Ok(new { Success = true, Data = result, Message = result.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }

        [HttpPost("reservations/{reservationId}/complete")]
        [Authorize(Roles = "Backoffice,GridOperator")]
        public async Task<IActionResult> Complete(string reservationId)
        {
            try
            {
                var result = await _service.CompleteTransferAsync(reservationId);
                return Ok(new { Success = true, Data = result, Message = result.Message });
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
    }
}
