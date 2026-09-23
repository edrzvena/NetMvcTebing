using Microsoft.EntityFrameworkCore;
using Tebing.Data;
using Tebing.Models;
using Tebing.Repositories.Interfaces;

namespace Tebing.Repositories.Implementations;

public class UserRepository(AppDbContext context) : Repository<User>(context), IUserRepository
{
    public async Task<User?> GetByEmailAsync(string email) =>
        await DbSet.FirstOrDefaultAsync(u => u.Email == email);

    public async Task<User?> GetByResetTokenAsync(string token) =>
        await DbSet.FirstOrDefaultAsync(u => u.ResetPasswordToken == token);
}
