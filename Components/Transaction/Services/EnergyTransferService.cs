/*
 * File: EnergyTransferService.cs
 * Description: Issues a one-time QR after a reservation is Approved, checks a
 *              scanned code against the stored token and the slot time, then
 *              starts and completes the energy transfer.
 */

using QRCoder;
using SmartSolarMicrogrid.API.Components.Transaction.DTOs;
using SmartSolarMicrogrid.API.Components.Transaction.Interfaces;
using SmartSolarMicrogrid.API.Components.Transaction.Models;

namespace SmartSolarMicrogrid.API.Components.Transaction.Services
{
    public class EnergyTransferService : IEnergyTransferService
    {
        private readonly IEnergyTransferRepository _transfers;
        private readonly IReservationLookup _reservations;

        public EnergyTransferService(IEnergyTransferRepository transfers, IReservationLookup reservations)
        {
            _transfers = transfers;
            _reservations = reservations;
        }

        public async Task<TransferResponse> IssueQrAsync(string reservationId)
        {
            var reservation = await RequireApprovedReservationAsync(reservationId);
            var existing = await _transfers.GetByReservationIdAsync(reservationId);
            if (existing != null)
            {
                return Map(existing, "QR was already issued for this booking.");
            }

            var token = Guid.NewGuid().ToString("N");
            var now = DateTime.UtcNow;
            var transfer = new EnergyTransfer
            {
                ReservationId = reservation.Id!,
                ProsumerNic = reservation.ProsumerNic,
                StationId = reservation.StationId,
                StationName = reservation.StationName,
                SlotId = reservation.SlotId,
                ReservationDate = reservation.ReservationDate,
                StartTime = reservation.StartTime,
                EndTime = reservation.EndTime,
                EnergyAmountKwh = reservation.EnergyAmountKwh,
                QrToken = token,
                QrPayload = $"SSM|{reservation.Id}|{token}",
                QrIssuedAt = now,
                TransferStatus = TransferStatusNames.QrIssued,
                CreatedAt = now,
                UpdatedAt = now
            };

            await _transfers.CreateAsync(transfer);
            return Map(transfer, "Booking confirmed. Show this QR to the grid operator at the slot time.");
        }

        public async Task<TransferResponse> GetConfirmationAsync(string reservationId, string? userId, string? role)
        {
            var transfer = await _transfers.GetByReservationIdAsync(reservationId);
            if (transfer == null)
            {
                throw new KeyNotFoundException("No QR has been issued for this booking yet.");
            }

            if (string.Equals(role, "Prosumer", StringComparison.OrdinalIgnoreCase))
            {
                var nic = string.IsNullOrWhiteSpace(userId)
                    ? null
                    : await _reservations.GetProsumerNicByUserIdAsync(userId);
                if (string.IsNullOrWhiteSpace(nic) || !string.Equals(nic, transfer.ProsumerNic, StringComparison.OrdinalIgnoreCase))
                {
                    throw new UnauthorizedAccessException("You can only view your own booking confirmation.");
                }
            }

            return Map(transfer, ConfirmationMessage(transfer.TransferStatus));
        }

        public async Task<TransferResponse> VerifyScanAsync(string qrPayload)
        {
            if (string.IsNullOrWhiteSpace(qrPayload))
            {
                throw new InvalidOperationException("QR payload is required.");
            }

            var parts = qrPayload.Trim().Split('|');
            if (parts.Length != 3 || parts[0] != "SSM" || string.IsNullOrWhiteSpace(parts[1]) || string.IsNullOrWhiteSpace(parts[2]))
            {
                throw new InvalidOperationException("This QR code is not a SolarGrid booking code.");
            }

            var transfer = await _transfers.GetByReservationIdAsync(parts[1]);
            if (transfer == null || !string.Equals(transfer.QrToken, parts[2], StringComparison.Ordinal))
            {
                throw new InvalidOperationException("QR code does not match a confirmed booking.");
            }

            if (transfer.TransferStatus == TransferStatusNames.Completed)
            {
                throw new InvalidOperationException("This energy transfer is already completed.");
            }

            if (transfer.TransferStatus == TransferStatusNames.InTransfer)
            {
                return Map(transfer, "This booking is already verified. Energy transfer is in progress.");
            }

            EnsureInsideSlotWindow(transfer);

            var now = DateTime.UtcNow;
            transfer.TransferStatus = TransferStatusNames.InTransfer;
            transfer.VerifiedAt = now;
            transfer.TransferStartedAt = now;
            transfer.UpdatedAt = now;
            await _transfers.UpdateAsync(transfer);

            return Map(transfer, "QR verified. Energy transfer has started.");
        }

        public async Task<TransferResponse> CompleteTransferAsync(string reservationId)
        {
            var transfer = await _transfers.GetByReservationIdAsync(reservationId);
            if (transfer == null)
            {
                throw new KeyNotFoundException("No transfer exists for this booking.");
            }

            if (transfer.TransferStatus == TransferStatusNames.Completed)
            {
                return Map(transfer, "Energy transfer is already completed.");
            }

            if (transfer.TransferStatus != TransferStatusNames.InTransfer)
            {
                throw new InvalidOperationException("Scan and verify the QR before completing the transfer.");
            }

            var marked = await _reservations.MarkCompletedAsync(reservationId);
            if (!marked)
            {
                throw new InvalidOperationException("The booking could not be completed. It must still be Approved.");
            }

            transfer.TransferStatus = TransferStatusNames.Completed;
            transfer.CompletedAt = DateTime.UtcNow;
            transfer.UpdatedAt = DateTime.UtcNow;
            await _transfers.UpdateAsync(transfer);

            return Map(transfer, "Energy transfer completed.");
        }

        private async Task<ReservationSnapshot> RequireApprovedReservationAsync(string reservationId)
        {
            var reservation = await _reservations.GetByIdAsync(reservationId);
            if (reservation == null || string.IsNullOrWhiteSpace(reservation.Id))
            {
                throw new KeyNotFoundException("Reservation not found.");
            }

            if (!string.Equals(reservation.Status, ReservationStatusNames.Approved, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"QR can be issued only for an Approved booking. Current status: {reservation.Status}.");
            }

            return reservation;
        }

        private static void EnsureInsideSlotWindow(EnergyTransfer transfer)
        {
            if (!TimeSpan.TryParse(transfer.StartTime, out var start) || !TimeSpan.TryParse(transfer.EndTime, out var end))
            {
                throw new InvalidOperationException("This booking does not have a valid slot time.");
            }

            var day = transfer.ReservationDate.Date;
            var windowStart = DateTime.SpecifyKind(day.Add(start), DateTimeKind.Utc);
            var windowEnd = DateTime.SpecifyKind(day.Add(end), DateTimeKind.Utc);
            if (windowEnd <= windowStart)
            {
                windowEnd = windowEnd.AddDays(1);
            }

            var now = DateTime.UtcNow;
            if (now < windowStart || now > windowEnd)
            {
                throw new InvalidOperationException("QR can be scanned only during the booked date and time.");
            }
        }

        private static string ConfirmationMessage(string status)
        {
            return status switch
            {
                TransferStatusNames.InTransfer => "QR verified. Energy transfer is in progress.",
                TransferStatusNames.Completed => "Energy transfer is completed.",
                _ => "Booking confirmed. Show this QR to the grid operator at the slot time."
            };
        }

        private static TransferResponse Map(EnergyTransfer transfer, string message)
        {
            return new TransferResponse
            {
                ReservationId = transfer.ReservationId,
                ProsumerNic = transfer.ProsumerNic,
                StationId = transfer.StationId,
                StationName = transfer.StationName,
                SlotId = transfer.SlotId,
                ReservationDate = transfer.ReservationDate,
                StartTime = transfer.StartTime,
                EndTime = transfer.EndTime,
                EnergyAmountKwh = transfer.EnergyAmountKwh,
                TransferStatus = transfer.TransferStatus,
                QrPayload = transfer.QrPayload,
                QrImageBase64 = BuildQrImage(transfer.QrPayload),
                QrIssuedAt = transfer.QrIssuedAt,
                VerifiedAt = transfer.VerifiedAt,
                TransferStartedAt = transfer.TransferStartedAt,
                CompletedAt = transfer.CompletedAt,
                Message = message
            };
        }

        private static string BuildQrImage(string payload)
        {
            using var generator = new QRCodeGenerator();
            using var data = generator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
            var png = new PngByteQRCode(data);
            return Convert.ToBase64String(png.GetGraphic(8));
        }
    }
}
