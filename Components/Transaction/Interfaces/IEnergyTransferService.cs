/*
 * File: IEnergyTransferService.cs
 * Description: Business rules for issuing a verified QR, scanning it inside the
 *              booked time window, and completing the energy transfer.
 */

using SmartSolarMicrogrid.API.Components.Transaction.DTOs;

namespace SmartSolarMicrogrid.API.Components.Transaction.Interfaces
{
    public interface IEnergyTransferService
    {
        Task<TransferResponse> IssueQrAsync(string reservationId);
        Task<TransferResponse> GetConfirmationAsync(string reservationId, string? userId, string? role);
        Task<TransferResponse> VerifyScanAsync(string qrPayload);
        Task<TransferResponse> CompleteTransferAsync(string reservationId);
    }
}
