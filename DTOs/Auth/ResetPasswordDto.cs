using System.ComponentModel.DataAnnotations;

namespace Tebing.DTOs.Auth;

public class ResetPasswordDto
{
    [Required]
    public string Token { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), MinLength(6), Display(Name = "Password Baru")]
    public string NewPassword { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), Compare(nameof(NewPassword), ErrorMessage = "Password gak sama."),
     Display(Name = "Ulangi Password Baru")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
