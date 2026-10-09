

using SmartSolarMicrogrid.API.Components.Transaction.DTOs;

namespace SmartSolarMicrogrid.API.Components.Transaction.Interfaces
{
    public interface IEnergyTransferService
    {
        Task<TransferResponse> IssueQrAsync(string reservationId);
        Task<TransferResponse> RegenerateQrAsync(string reservationId);
        Task<TransferResponse> UpdateQrAsync(string reservationId, UpdateTransferRequest request);
        Task DeleteQrAsync(string reservationId);
        Task<TransferResponse> GetConfirmationAsync(string reservationId, string? userId, string? role);
        Task<TransferResponse> VerifyScanAsync(string qrPayload);
        Task<TransferResponse> CompleteTransferAsync(string reservationId);
    }
}
