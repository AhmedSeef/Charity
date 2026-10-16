using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Charity.Domain.Interfaces;
using Charity.Infrastructure.Data.Context;

namespace Charity.Infrastructure.Data.Repositories;

/// <summary>
/// Generic repository implementing EF Core operations returning ONLY entities.
/// </summary>
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly CharityDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(CharityDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(string id)
    {
        // FindAsync may return an already tracked soft-deleted entity and bypass filters.
        return await _dbSet.Where(e => EF.Property<string>(e, "Id") == id).FirstOrDefaultAsync();
    }

    public virtual async Task<IReadOnlyList<T>> GetAllAsync()
    {
        return await _dbSet.AsNoTracking().ToListAsync();
    }

    public virtual async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.Where(predicate).AsNoTracking().ToListAsync();
    }

    public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate);
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        return entity;
    }

    public virtual Task UpdateAsync(T entity)
    {
        _context.Entry(entity).State = EntityState.Modified;
        return Task.CompletedTask;
    }

    public virtual Task DeleteAsync(T entity)
    {
        if (entity is Charity.Domain.Entities.BaseEntity baseEntity)
        {
            baseEntity.IsDeleted = true;
            baseEntity.DeletedAt = DateTime.UtcNow;
            _context.Entry(entity).State = EntityState.Modified;
        }
        else
        {
            _dbSet.Remove(entity);
        }
        return Task.CompletedTask;
    }

    public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
    {
        if (predicate == null)
            return await _dbSet.CountAsync();

        return await _dbSet.CountAsync(predicate);
    }

    public virtual IQueryable<T> Query()
    {
        return _dbSet.AsQueryable();
    }
}
