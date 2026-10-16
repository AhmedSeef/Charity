using System.Linq.Expressions;
using Charity.Application.Services.Interfaces;
using Charity.Domain.Entities;
using Charity.Domain.Interfaces;

namespace Charity.UnitTests;

// These doubles isolate application rules; real EF/SQL behavior belongs to integration tests.
internal sealed class FakeRepository<T> : IRepository<T> where T : BaseEntity
{
    public List<T> Items { get; } = [];
    public int AddCalls { get; private set; }
    public int UpdateCalls { get; private set; }
    public int DeleteCalls { get; private set; }
    public FakeRepository(params T[] items) => Items.AddRange(items);
    public IQueryable<T> Query() => Items.Where(x => !x.IsDeleted).AsQueryable();
    public Task<T?> GetByIdAsync(string id) => Task.FromResult(Query().FirstOrDefault(x => x.Id == id));
    public Task<IReadOnlyList<T>> GetAllAsync() => Task.FromResult<IReadOnlyList<T>>(Query().ToList());
    public Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate) => Task.FromResult<IReadOnlyList<T>>(Query().Where(predicate).ToList());
    public Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate) => Task.FromResult(Query().FirstOrDefault(predicate));
    public Task<T> AddAsync(T entity)
    {
        if (Items.Any(x => x.Id == entity.Id)) throw new InvalidOperationException("Duplicate entity key.");
        Items.Add(entity); AddCalls++; return Task.FromResult(entity);
    }
    public Task UpdateAsync(T entity) { UpdateCalls++; return Task.CompletedTask; }
    public Task DeleteAsync(T entity) { entity.IsDeleted = true; entity.DeletedAt = DateTime.UtcNow; DeleteCalls++; return Task.CompletedTask; }
    public Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null) => Task.FromResult(predicate == null ? Query().Count() : Query().Count(predicate));
}

internal sealed class FakeUnitOfWork : IUnitOfWork
{
    public int SaveCalls { get; private set; }
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) { SaveCalls++; return Task.FromResult(1); }
    public IRepository<T> Repository<T>() where T : class => throw new NotSupportedException();
    public void Dispose() { }
}

internal sealed class FakeCurrentUser : ICurrentUserService
{
    public string? UserId => "tester";
    public string? Username => "tester";
    public string? CharityId { get; set; } = "c1";
    public string? CharityName => "جمعية الاختبار";
    public string? Role => "Admin";
    public bool IsAuthenticated => true;
}
