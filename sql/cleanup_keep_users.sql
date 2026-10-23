SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;
-- Preserve Identity accounts, roles, role assignments, EF migrations, and charities referenced by users.
-- Run only against the intended CharityDb after a verified full backup.
SET XACT_ABORT ON;
BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @UsersBefore int = (SELECT COUNT(*) FROM dbo.Users);
    DECLARE @UserRolesBefore int = (SELECT COUNT(*) FROM dbo.UserRoles);
    DECLARE @LinkedUsersBefore int = (SELECT COUNT(*) FROM dbo.Users WHERE CharityId IS NOT NULL);

    DELETE FROM dbo.Distributions;
    DELETE FROM dbo.Donations;
    DELETE FROM dbo.BeneficiaryHeadHistories;
    DELETE FROM dbo.BeneficiaryFamilyMembers;
    DELETE FROM dbo.Beneficiaries;
    DELETE FROM dbo.InventoryItems;
    DELETE FROM dbo.StaffMembers;
    DELETE FROM dbo.CharitySettings;
    DELETE FROM dbo.AddressRecords;
    DELETE FROM dbo.VillageAreas;
    DELETE FROM dbo.Villages;
    DELETE FROM dbo.Districts;
    DELETE FROM dbo.Governorates;
    DELETE FROM dbo.ExpenseCategories;
    DELETE FROM dbo.SuspensionReasons;
    DELETE FROM dbo.SeparationReasons;
    DELETE FROM dbo.Charities
    WHERE Id NOT IN (SELECT CharityId FROM dbo.Users WHERE CharityId IS NOT NULL);

    IF (SELECT COUNT(*) FROM dbo.Users) <> @UsersBefore
       OR (SELECT COUNT(*) FROM dbo.UserRoles) <> @UserRolesBefore
       OR (SELECT COUNT(*) FROM dbo.Users WHERE CharityId IS NOT NULL) <> @LinkedUsersBefore
       OR EXISTS (SELECT 1 FROM dbo.Distributions)
       OR EXISTS (SELECT 1 FROM dbo.Donations)
       OR EXISTS (SELECT 1 FROM dbo.Beneficiaries)
       OR EXISTS (SELECT 1 FROM dbo.InventoryItems)
       OR EXISTS (SELECT 1 FROM dbo.ExpenseCategories)
       THROW 51000, 'Cleanup verification failed; transaction rolled back.', 1;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;