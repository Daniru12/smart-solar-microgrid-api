/*
 * File: TransferDtos.cs
 * Description: Request and response shapes for issuing a QR, verifying a scan,
 *              and reading energy-transfer status.
 */

namespace SmartSolarMicrogrid.API.Components.Transaction.DTOs
{
    public class VerifyQrRequest
    {
        public string QrPayload { get; set; } = string.Empty;
    }

    public class TransferResponse
    {
        public string ReservationId { get; set; } = string.Empty;
        public string ProsumerNic { get; set; } = string.Empty;
        public string StationId { get; set; } = string.Empty;
        public string StationName { get; set; } = string.Empty;
        public string SlotId { get; set; } = string.Empty;
        public DateTime ReservationDate { get; set; }
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public double EnergyAmountKwh { get; set; }
        public string TransferStatus { get; set; } = string.Empty;
        public string QrPayload { get; set; } = string.Empty;
        public string QrImageBase64 { get; set; } = string.Empty;
        public DateTime QrIssuedAt { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public DateTime? TransferStartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
