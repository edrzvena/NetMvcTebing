using Microsoft.EntityFrameworkCore;
using Tebing.Models;

namespace Tebing.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Mountain> Mountains => Set<Mountain>();
    public DbSet<HikingRoute> HikingRoutes => Set<HikingRoute>();
}