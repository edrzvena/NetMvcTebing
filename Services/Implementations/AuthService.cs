using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Tebing.DTOs.Auth;
using Tebing.Models;
using Tebing.Repositories.Interfaces;
using Tebing.Services.Interfaces;

namespace Tebing.Services.Implementations;

public class AuthService(IUserRepository userRepository, IHttpContextAccessor httpContextAccessor)
    : IAuthService
{
    private static readonly PasswordHasher<User> PasswordHasher = new();
    private static readonly TimeSpan ResetTokenLifetime = TimeSpan.FromHours(1);

    public async Task<AuthResult> RegisterAsync(RegisterDto dto)
    {
        var existing = await userRepository.GetByEmailAsync(dto.Email);
        if (existing is not null)
            return new AuthResult(false, "Email sudah terdaftar.");

        var user = new User
        {
            Email = dto.Email,
            FullName = dto.FullName
        };
        user.PasswordHash = PasswordHasher.HashPassword(user, dto.Password);

        await userRepository.AddAsync(user);
        await userRepository.SaveChangesAsync();

        return new AuthResult(true);
    }

    public async Task<AuthResult> LoginAsync(LoginDto dto)
    {
        var user = await userRepository.GetByEmailAsync(dto.Email);
        if (user is null)
            return new AuthResult(false, "Email atau password salah.");

        var verification = PasswordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
        if (verification == PasswordVerificationResult.Failed)
            return new AuthResult(false, "Email atau password salah.");

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Role, user.Role.ToString())
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        var httpContext = httpContextAccessor.HttpContext!;
        await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
            new AuthenticationProperties { IsPersistent = dto.RememberMe });

        return new AuthResult(true);
    }

    public async Task LogoutAsync()
    {
        var httpContext = httpContextAccessor.HttpContext!;
        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }

    public async Task<string?> GenerateResetTokenAsync(string email)
    {
        var user = await userRepository.GetByEmailAsync(email);
        if (user is null)
            return null;

        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        user.ResetPasswordToken = token;
        user.PasswordResetTokenExpireAt = DateTime.UtcNow.Add(ResetTokenLifetime);

        userRepository.Update(user);
        await userRepository.SaveChangesAsync();

        return token;
    }

    public async Task<AuthResult> ResetPasswordAsync(ResetPasswordDto dto)
    {
        var user = await userRepository.GetByResetTokenAsync(dto.Token);
        if (user is null || user.PasswordResetTokenExpireAt is null ||
            user.PasswordResetTokenExpireAt < DateTime.UtcNow)
            return new AuthResult(false, "Token reset password tidak valid atau sudah kedaluwarsa.");

        user.PasswordHash = PasswordHasher.HashPassword(user, dto.NewPassword);
        user.ResetPasswordToken = null;
        user.PasswordResetTokenExpireAt = null;

        userRepository.Update(user);
        await userRepository.SaveChangesAsync();

        return new AuthResult(true);
    }
}
