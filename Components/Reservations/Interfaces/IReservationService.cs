

using SmartSolarMicrogrid.API.Components.Reservations.DTOs;

namespace SmartSolarMicrogrid.API.Components.Reservations.Interfaces
{

    public interface IReservationService
    {

        Task<ReservationResponseDto> CreateReservationAsync(CreateReservationDto dto);

        Task<ReservationResponseDto> UpdateReservationAsync(string id, UpdateReservationDto dto);

        Task<ReservationResponseDto> CancelReservationAsync(string id, string? operatorStationId = null);

        Task<ReservationResponseDto> ApproveReservationAsync(string id, string? operatorStationId = null);

        Task<ReservationResponseDto> ValidateQrAsync(string id, string? operatorStationId = null);

        Task<ReservationResponseDto> CompleteReservationAsync(string id, string? operatorStationId = null);

        Task DeleteReservationAsync(string id);

        Task<ReservationResponseDto> GetByIdAsync(string id);

        Task<List<ReservationResponseDto>> GetAllAsync(string? operatorStationId = null);

        Task<List<ReservationResponseDto>> GetByProsumerNicAsync(string nic);

        Task<List<ReservationResponseDto>> GetPendingAsync(string? operatorStationId = null);

        Task<List<ReservationResponseDto>> SearchAsync(ReservationSearchDto searchDto, string? operatorStationId = null);

        Task<DashboardSummaryDto> GetDashboardSummaryAsync(string? operatorStationId = null);

        Task<bool> HasActiveReservationsForStationAsync(string stationId);
    }
}

