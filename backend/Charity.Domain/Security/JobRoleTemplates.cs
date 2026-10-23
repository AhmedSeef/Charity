namespace Charity.Domain.Security;

public static class JobRoleTemplates
{
    public const string SocialWorker = "SocialWorker";
    public const string DataEntry = "DataEntry";
    public const string DisbursementOfficer = "DisbursementOfficer";
    public const string Storekeeper = "Storekeeper";
    public const string Accountant = "Accountant";
    public const string Custom = "Custom";

    public static readonly IReadOnlyDictionary<string, string[]> TemplatePermissions = new Dictionary<string, string[]>
    {
        [SocialWorker] = new[]
        {
            AppPermissions.BeneficiariesView,
            AppPermissions.BeneficiariesViewFull,
            AppPermissions.BeneficiariesCreate,
            AppPermissions.BeneficiariesEdit
        },
        [DataEntry] = new[]
        {
            AppPermissions.BeneficiariesView,
            AppPermissions.BeneficiariesCreate
        },
        [DisbursementOfficer] = new[]
        {
            AppPermissions.BeneficiariesView,
            AppPermissions.DisbursementExecute,
            AppPermissions.DisbursementView,
            AppPermissions.InventoryView
        },
        [Storekeeper] = new[]
        {
            AppPermissions.InventoryView,
            AppPermissions.InventoryManage,
            AppPermissions.DisbursementView
        },
        [Accountant] = new[]
        {
            AppPermissions.DonationsView,
            AppPermissions.DonationsCreate,
            AppPermissions.ExpensesManage,
            AppPermissions.DisbursementView
        },
        [Custom] = Array.Empty<string>()
    };
}
