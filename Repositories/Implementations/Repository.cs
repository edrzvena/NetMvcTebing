using Microsoft.EntityFrameworkCore;
using Tebing.Data;
using Tebing.Repositories.Interfaces;

namespace Tebing.Repositories.Implementations;

public class Repository<T>(AppDbContext context) : IRepository<T> where T : class
{
    protected readonly AppDbContext Context = context;
    protected readonly DbSet<T> DbSet = context.Set<T>();

    public async Task<T?> GetByIdAsync(int id) => await DbSet.FindAsync(id);
    public async Task<List<T>> GetAllAsync() => await DbSet.ToListAsync();
    public async Task AddAsync(T entity) => await DbSet.AddAsync(entity);
    public void Update(T entity) => DbSet.Update(entity);
    public void Remove(T entity) => DbSet.Remove(entity);
    public async Task SaveChangesAsync() => await Context.SaveChangesAsync();
}
