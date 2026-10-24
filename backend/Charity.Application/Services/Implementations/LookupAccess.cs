using Charity.Application.Services.Interfaces;
using Charity.Domain.Exceptions;
using Charity.Domain.Security;

namespace Charity.Application.Services.Implementations;

internal static class LookupAccess
{
    public static void DemandManager(ICurrentUserService user, bool allowAdmin = true)
    {
        if (user.Role != "CharityManager" && !(allowAdmin && user.Role == "Admin"))
            throw new UnauthorizedDomainException("غير مصرح: هذه العملية متاحة لمدير الجمعية فقط.");
    }

    public static void DemandGeographyWrite(ICurrentUserService user)
    {
        if (!user.HasPermission(AppPermissions.BeneficiariesCreate) && !user.HasPermission(AppPermissions.BeneficiariesEdit))
            throw new UnauthorizedDomainException("غير مصرح: إدارة المناطق والعناوين تتطلب صلاحية إضافة أو تعديل المستفيدين.");
    }

    public static void DemandGeographyRead(ICurrentUserService user)
    {
        if (!user.HasPermission(AppPermissions.BeneficiariesView) && !user.HasPermission(AppPermissions.BeneficiariesCreate)
            && !user.HasPermission(AppPermissions.BeneficiariesEdit) && !user.HasPermission(AppPermissions.DisbursementExecute))
            throw new UnauthorizedDomainException("غير مصرح: ليس لديك صلاحية الاطلاع على المناطق والعناوين.");
    }
}
