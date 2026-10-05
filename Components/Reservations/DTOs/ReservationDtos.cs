

using System.ComponentModel.DataAnnotations;

namespace SmartSolarMicrogrid.API.Components.Reservations.DTOs
{

    public class CreateReservationDto
    {

        [Required]
        public string ProsumerNic { get; set; } = string.Empty;

        [Required]
        public string StationId { get; set; } = string.Empty;

        [Required]
        public string SlotId { get; set; } = string.Empty;

        [Required]
        public DateTime ReservationDate { get; set; }

        [Required]
        [Range(0.1, double.MaxValue, ErrorMessage = "Energy amount must be greater than 0.")]
        public double EnergyAmountKwh { get; set; }

        public string? Notes { get; set; }
    }

    public class UpdateReservationDto
    {

        [Required]
        public DateTime ReservationDate { get; set; }

        [Required]
        public string SlotId { get; set; } = string.Empty;

        [Required]
        [Range(0.1, double.MaxValue, ErrorMessage = "Energy amount must be greater than 0.")]
        public double EnergyAmountKwh { get; set; }

        public string? Notes { get; set; }
    }

    public class ReservationSearchDto
    {

        public string? Nic { get; set; }

        public string? StationId { get; set; }

        public string? Status { get; set; }

        public DateTime? From { get; set; }

        public DateTime? To { get; set; }
    }

    public class ReservationResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string ProsumerNic { get; set; } = string.Empty;
        public string StationId { get; set; } = string.Empty;
        public string StationName { get; set; } = string.Empty;
        public string SlotId { get; set; } = string.Empty;
        public DateTime ReservationDate { get; set; }
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public double EnergyAmountKwh { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }

    public class DashboardSummaryDto
    {

        public long PendingCount { get; set; }

        public long ApprovedFutureCount { get; set; }
    }
}
