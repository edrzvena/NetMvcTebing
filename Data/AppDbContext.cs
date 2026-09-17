using Microsoft.EntityFrameworkCore;

namespace Tebing.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    
}