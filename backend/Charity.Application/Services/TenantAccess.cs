using Charity.Application.Services.Interfaces;
using Charity.Domain.Exceptions;

namespace Charity.Application.Services;

internal static class TenantAccess
{
    public static string RequireCharity(ICurrentUserService user) => !string.IsNullOrWhiteSpace(user.CharityId)
        ? user.CharityId : throw new UnauthorizedDomainException("هذه العملية تتطلب حساب جمعية صالحاً.");

    // Platform administrators retain read-only oversight; charity accounts only see their own records.
    public static bool CanRead(string? owner, ICurrentUserService user) =>
        (user.Role == "Admin" && string.IsNullOrWhiteSpace(user.CharityId)) ||
        (!string.IsNullOrWhiteSpace(user.CharityId) && owner == user.CharityId);

    public static void DemandOwner(string? owner, ICurrentUserService user)
    {
        if ((!string.IsNullOrWhiteSpace(user.CharityId) && owner == user.CharityId) ||
            (owner == null && user.Role == "Admin" && string.IsNullOrWhiteSpace(user.CharityId))) return;
        throw new UnauthorizedDomainException("غير مصرح: هذه البيانات تخص جمعية أخرى.");
    }
}
