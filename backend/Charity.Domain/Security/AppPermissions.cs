namespace Charity.Domain.Security;

public static class AppPermissions
{
    // المستفيدين
    public const string BeneficiariesView = "beneficiaries.view";
    public const string BeneficiariesViewFull = "beneficiaries.view_full";
    public const string BeneficiariesCreate = "beneficiaries.create";
    public const string BeneficiariesEdit = "beneficiaries.edit";
    public const string BeneficiariesDelete = "beneficiaries.delete";

    // الصرف والتوزيع
    public const string DisbursementExecute = "disbursement.execute";
    public const string DisbursementView = "disbursement.view";

    // المخازن والأصناف
    public const string InventoryView = "inventory.view";
    public const string InventoryManage = "inventory.manage";

    // التبرعات
    public const string DonationsView = "donations.view";
    public const string DonationsCreate = "donations.create";

    // المصروفات
    public const string ExpensesManage = "expenses.manage";

    // إدارة فريق العمل والمستخدمين
    public const string StaffManage = "staff.manage";

    public static readonly IReadOnlyList<string> All = new[]
    {
        BeneficiariesView,
        BeneficiariesViewFull,
        BeneficiariesCreate,
        BeneficiariesEdit,
        BeneficiariesDelete,
        DisbursementExecute,
        DisbursementView,
        InventoryView,
        InventoryManage,
        DonationsView,
        DonationsCreate,
        ExpensesManage,
        StaffManage
    };
}
