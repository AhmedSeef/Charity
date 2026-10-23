namespace Charity.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<T> Repository<T>() where T : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    void ChangeInventoryBalance(Charity.Domain.Entities.InventoryItem item, double delta, bool allowNegativeBalance);
    void CheckCashDisbursement(string charityId, decimal amount, bool allowNegativeBalance);
    decimal? CashBalanceAfterDisbursement { get; }
}
