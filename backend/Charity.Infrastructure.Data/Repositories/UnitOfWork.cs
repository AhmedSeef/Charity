using System.Collections.Concurrent;
using Charity.Domain.Interfaces;
using Charity.Infrastructure.Data.Context;

namespace Charity.Infrastructure.Data.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly CharityDbContext _context;
    private readonly ConcurrentDictionary<string, object> _repositories = new();

    public UnitOfWork(CharityDbContext context)
    {
        _context = context;
    }

    public IRepository<T> Repository<T>() where T : class
    {
        var typeName = typeof(T).Name;
        return (IRepository<T>)_repositories.GetOrAdd(typeName, _ => new Repository<T>(_context));
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
