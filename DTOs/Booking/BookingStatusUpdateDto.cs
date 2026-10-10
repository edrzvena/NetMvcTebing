using System.ComponentModel.DataAnnotations;
using Tebing.Common.Enums;

namespace Tebing.DTOs.Booking;

public class BookingStatusUpdateDto
{
    public int BookingId { get; set; }
    public BookingStatus Status { get; set; }

    [MaxLength(500)] public string? RejectionReason { get; set; }
}
