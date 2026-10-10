using System.ComponentModel.DataAnnotations;

namespace Tebing.DTOs.Payment;

public class PaymentUploadDto
{
    public int BookingId { get; set; }

    [Required(ErrorMessage = "Pilih file bukti transfer.")]
    [Display(Name = "Bukti Transfer")]
    public IFormFile? ProofFile { get; set; }
}
