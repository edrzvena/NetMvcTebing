using System.ComponentModel.DataAnnotations;

namespace Tebing.DTOs.Auth;

public class RegisterDto
{
    [Required, EmailAddress, MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required, MaxLength(100), Display(Name = "Nama Lengkap")]
    public string FullName { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), Compare(nameof(Password), ErrorMessage = "Password gak sama."),
     Display(Name = "Ulangi Password")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
