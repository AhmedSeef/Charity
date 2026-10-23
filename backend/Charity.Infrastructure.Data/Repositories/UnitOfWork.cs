using System.Collections.Concurrent;
using Charity.Domain.Interfaces;
using Charity.Infrastructure.Data.Context;
using Charity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Charity.Domain.Exceptions;
using Charity.Application.Services.Interfaces;
using Microsoft.Data.SqlClient;

namespace Charity.Infrastructure.Data.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly CharityDbContext _context;
    private readonly ICurrentUserService? _currentUser;
    private readonly ConcurrentDictionary<string, object> _repositories = new();
    private readonly List<(InventoryItem Item, double Delta, bool AllowNegative)> _inventoryChanges = new();
    private (string CharityId, decimal Amount, bool AllowNegative)? _cashDisbursement;
    public decimal? CashBalanceAfterDisbursement { get; private set; }

    public UnitOfWork(CharityDbContext context, ICurrentUserService? currentUser = null)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public IRepository<T> Repository<T>() where T : class
    {
        var typeName = typeof(T).Name;
        return (IRepository<T>)_repositories.GetOrAdd(typeName, _ => new Repository<T>(_context));
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        _context.ChangeTracker.DetectChanges();
        var identityIds = _context.ChangeTracker.Entries()
            .Where(e => (e.Entity is Beneficiary || e.Entity is BeneficiaryFamilyMember) &&
                (e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted))
            .SelectMany(e => new[] { e.Property("NationalId").CurrentValue as string, e.Property("NationalId").OriginalValue as string })
            .Where(id => !string.IsNullOrWhiteSpace(id)).Select(id => id!).Distinct().OrderBy(id => id, StringComparer.Ordinal).ToList();
        if (_inventoryChanges.Count == 0 && _cashDisbursement == null && identityIds.Count == 0)
            return await _context.SaveChangesAsync(cancellationToken);

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        foreach (var nationalId in identityIds)
        {
            var resource = $"CharityIdentity:{nationalId}";
            await _context.Database.ExecuteSqlInterpolatedAsync($@"
                DECLARE @result int;
                EXEC @result = sp_getapplock @Resource={resource}, @LockMode='Exclusive', @LockOwner='Transaction', @LockTimeout=10000;
                IF @result < 0 THROW 51000, 'تعذر التحقق من الرقم القومي. حاول مرة أخرى.', 1;", cancellationToken);
        }
        if (_cashDisbursement is { } cash)
        {
            // Serialize the check and ledger insert for this charity, including across API processes.
            var resource = $"CharityCash:{cash.CharityId}";
            await _context.Database.ExecuteSqlInterpolatedAsync($@"
                DECLARE @result int;
                EXEC @result = sp_getapplock @Resource={resource}, @LockMode='Exclusive', @LockOwner='Transaction', @LockTimeout=10000;
                IF @result < 0 THROW 51000, 'تعذر الحصول على قفل الرصيد النقدي. حاول مرة أخرى.', 1;", cancellationToken);
            var incoming = await _context.Donations.Where(d => d.CharityId == cash.CharityId &&
                (d.DonationKind == "نقدي" || d.DonationKind == ""))
                .SumAsync(d => d.Amount ?? 0, cancellationToken);
            var outgoing = await _context.Distributions.Where(d => d.CharityId == cash.CharityId && d.Kind == "إعانة نقدية")
                .SumAsync(d => d.Amount ?? 0, cancellationToken);
            var remaining = incoming - outgoing - cash.Amount;
            if (remaining < 0 && !cash.AllowNegative)
                throw new BalanceConfirmationRequiredException(
                    $"الرصيد النقدي المسجل الآن {incoming - outgoing} جنيه والمطلوب {cash.Amount} جنيه. سيصبح الرصيد {remaining} جنيه. هل تريد الاستمرار وتسجيل العجز؟");
            CashBalanceAfterDisbursement = remaining;
        }
        foreach (var (item, delta, allowNegative) in _inventoryChanges)
        {
            // Apply the delta in SQL so concurrent incoming and outgoing stock cannot overwrite each other.
            var stock = _context.InventoryItems.Where(i => i.Id == item.Id && (allowNegative || i.Quantity + delta >= 0));
            var actorId = _currentUser?.IsAuthenticated == true ? _currentUser.UserId : null;
            var actorName = _currentUser?.FullName ?? _currentUser?.Username;
            var changedAt = DateTime.UtcNow;
            var updated = actorId == null
                ? await stock.ExecuteUpdateAsync(setters => setters.SetProperty(i => i.Quantity, i => i.Quantity + delta), cancellationToken)
                : await stock.ExecuteUpdateAsync(setters => setters
                    .SetProperty(i => i.Quantity, i => i.Quantity + delta)
                    .SetProperty(i => i.UpdatedAt, changedAt)
                    .SetProperty(i => i.UpdatedBy, actorId)
                    .SetProperty(i => i.UpdatedByName, actorName), cancellationToken);
            if (updated != 1)
            {
                await _context.Entry(item).ReloadAsync(cancellationToken);
                throw new BalanceConfirmationRequiredException(
                    $"تغير رصيد الصنف ({item.ItemName}). المسجل الآن {item.Quantity} {item.Unit} والمطلوب {-delta}. سيصبح الرصيد {item.Quantity + delta}. هل تريد الاستمرار وتسجيل العجز؟");
            }
            await _context.Entry(item).ReloadAsync(cancellationToken);
            if (actorId != null)
                _context.AuditLogs.Add(new AuditLog
                {
                    OccurredAt = changedAt, CharityId = item.CharityId, UserId = actorId,
                    Username = _currentUser?.Username ?? actorId, ActorName = actorName ?? actorId,
                    Action = "Update", EntityType = nameof(InventoryItem), EntityId = item.Id,
                    EntityName = item.ItemName
                });
        }
        int result;
        try
        {
            result = await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (identityIds.Count > 0 && ex.InnerException is SqlException sql &&
            (sql.Number == 2601 || sql.Number == 2627) && sql.Message.Contains("NationalId", StringComparison.Ordinal))
        {
            throw new ValidationDomainException("الرقم القومي مسجل مسبقاً. لا يمكن تسجيل نفس الشخص في أكثر من ملف أو جمعية.");
        }
        if (identityIds.Count > 0)
        {
            // Enforce the invariant across BOTH tables, including simultaneous head/member registration.
            var heads = await _context.Beneficiaries.Where(b => identityIds.Contains(b.NationalId)).Select(b => b.NationalId).ToListAsync(cancellationToken);
            var members = await _context.BeneficiaryFamilyMembers.Where(m => identityIds.Contains(m.NationalId)).Select(m => m.NationalId).ToListAsync(cancellationToken);
            if (heads.Concat(members).GroupBy(id => id).Any(group => group.Count() > 1))
                throw new ValidationDomainException("الرقم القومي مسجل مسبقاً كصاحب ملف أو فرد أسرة. لا يمكن تسجيل نفس الشخص في أكثر من ملف أو جمعية.");
        }
        await transaction.CommitAsync(cancellationToken);
        _inventoryChanges.Clear();
        _cashDisbursement = null;
        return result;
    }

    public void ChangeInventoryBalance(InventoryItem item, double delta, bool allowNegativeBalance) => _inventoryChanges.Add((item, delta, allowNegativeBalance));
    public void CheckCashDisbursement(string charityId, decimal amount, bool allowNegativeBalance) =>
        _cashDisbursement = (charityId, amount, allowNegativeBalance);

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
