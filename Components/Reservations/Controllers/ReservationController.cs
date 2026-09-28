/*
 * File: ReservationController.cs
 * Author: Upasama (Member 3 - Reservation & Booking Management)
 * Description: ASP.NET Core Web API Controller for all reservation endpoints.
 *              Acts as a thin routing layer only - all business logic is in ReservationService.
 *              Role-based access control using [Authorize(Roles = "...")] from Member 1 JWT.
 *              Roles from Member 1: Backoffice | GridOperator | Prosumer
 * Created: 2026
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSolarMicrogrid.API.Components.Reservations.DTOs;
using SmartSolarMicrogrid.API.Components.Reservations.Interfaces;

namespace SmartSolarMicrogrid.API.Components.Reservations.Controllers
{
    /// <summary>
    /// REST API Controller for Energy Reservation management.
    /// Base route: /api/reservations
    /// Role-based access: uses JWT roles set by Member 1 AuthService.
    /// </summary>
    [ApiController]
    [Route("api/reservations")]
    [Authorize] // All endpoints require a valid JWT token (Member 1 auth)
    public class ReservationController : ControllerBase
    {
        private readonly IReservationService _service;

        /// <summary>
        /// Constructor: IReservationService injected by ASP.NET Core DI.
        /// </summary>
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
            return null; // Backoffice role or Prosumer gets null (not filtered by operator station)
        }

        // ============================================================
        // GET ENDPOINTS - Query / Read
        // ============================================================

        /// <summary>
        /// GET /api/reservations
        /// Returns all reservations. Accessible by Backoffice and GridOperator only.
        /// Web app: Reservation List page (admin view).
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Backoffice,GridOperator")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync(GetOperatorStationId());
            return Ok(new { Success = true, Data = result });
        }

        /// <summary>
        /// GET /api/reservations/{id}
        /// Returns a single reservation by MongoDB ObjectId.
        /// Accessible by all authenticated roles (Prosumers can view their own).
        /// </summary>
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

        /// <summary>
        /// GET /api/reservations/pending
        /// Returns all Pending reservations sorted oldest first.
        /// Web app: Pending Reservations management page.
        /// Accessible by Backoffice and GridOperator only.
        /// </summary>
        [HttpGet("pending")]
        [Authorize(Roles = "Backoffice,GridOperator")]
        public async Task<IActionResult> GetPending()
        {
            var result = await _service.GetPendingAsync(GetOperatorStationId());
            return Ok(new { Success = true, Data = result });
        }

        /// <summary>
        /// GET /api/reservations/prosumer/{nic}
        /// Returns all reservations for a specific Prosumer NIC (booking history).
        /// Mobile app: Booking History and Pending Bookings screens.
        /// Accessible by all roles (Prosumer views own, Operators can view any).
        /// </summary>
        [HttpGet("prosumer/{nic}")]
        public async Task<IActionResult> GetByProsumerNic(string nic)
        {
            var result = await _service.GetByProsumerNicAsync(nic);
            return Ok(new { Success = true, Data = result });
        }

        /// <summary>
        /// GET /api/reservations/search
        /// Flexible search endpoint with optional query parameters.
        /// Supports: ?nic=, ?stationId=, ?status=, ?from=, ?to=
        /// Web app: Search/Filter reservations page.
        /// Mobile app: Search bookings screen.
        /// </summary>
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

        /// <summary>
        /// GET /api/reservations/prosumer/{nic}/search
        /// Prosumer-specific search (for mobile booking search screen).
        /// Accessible by Prosumer role.
        /// </summary>
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
            var result = await _service.SearchAsync(searchDto); // Prosumers don't filter by operator station
            return Ok(new { Success = true, Data = result });
        }

        /// <summary>
        /// GET /api/reservations/dashboard
        /// Returns dashboard summary counts: PendingCount + ApprovedFutureCount.
        /// Web app: Dashboard widget. Accessible by Backoffice and GridOperator.
        /// </summary>
        [HttpGet("dashboard")]
        [Authorize(Roles = "Backoffice,GridOperator")]
        public async Task<IActionResult> GetDashboardSummary()
        {
            var result = await _service.GetDashboardSummaryAsync(GetOperatorStationId());
            return Ok(new { Success = true, Data = result });
        }

        // ============================================================
        // POST ENDPOINTS - Create
        // ============================================================

        /// <summary>
        /// POST /api/reservations
        /// Creates a new reservation. Prosumer only.
        /// Mobile app: Create Booking screen.
        /// Business Rules enforced in service:
        ///   - Date within 7 days.
        ///   - No slot conflict for same date.
        /// Returns HTTP 201 Created with the new reservation.
        /// </summary>
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

        // ============================================================
        // PUT ENDPOINTS - Update / Status Changes
        // ============================================================

        /// <summary>
        /// PUT /api/reservations/{id}
        /// Updates an existing Pending reservation. Prosumer only.
        /// Mobile app: Modify Booking screen.
        /// Business Rule enforced in service: >= 12 hours before reservation.
        /// </summary>
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

        /// <summary>
        /// PUT /api/reservations/{id}/approve
        /// Approves a Pending reservation. Backoffice or GridOperator only.
        /// Web app: Pending Reservations page - Approve button.
        /// </summary>
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

        /// <summary>
        /// GET /api/reservations/{id}/validate-qr
        /// Validates a reservation QR code for energy transfer.
        /// GridOperator only.
        /// </summary>
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

        /// <summary>
        /// PUT /api/reservations/{id}/complete
        /// Completes an Approved reservation after QR scan and energy transfer.
        /// GridOperator only.
        /// </summary>
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

        /// <summary>
        /// PUT /api/reservations/{id}/cancel
        /// Cancels a Pending or Approved reservation. All roles can cancel.
        /// Mobile app: Cancel Booking screen. Web app: Cancel button in management view.
        /// Business Rule enforced in service: >= 12 hours before reservation.
        /// </summary>
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

        // ============================================================
        // DELETE ENDPOINTS - Hard Delete
        // ============================================================

        /// <summary>
        /// DELETE /api/reservations/{id}
        /// Hard deletes a reservation. Backoffice only.
        /// Web app: Admin delete option.
        /// </summary>
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

