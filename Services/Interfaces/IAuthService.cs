using Tebing.DTOs.Auth;

namespace Tebing.Services.Interfaces;

public record AuthResult(bool Success, string? Error = null);

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(RegisterDto dto);
    Task<AuthResult> LoginAsync(LoginDto dto);
    Task LogoutAsync();
    Task<string?> GenerateResetTokenAsync(string email);
    Task<AuthResult> ResetPasswordAsync(ResetPasswordDto dto);
}
