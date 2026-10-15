using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Charity.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAdvancedSocialSurveyAndPovertyDimensions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EducationStage",
                table: "BeneficiaryFamilyMembers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsChildLabor",
                table: "BeneficiaryFamilyMembers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSchoolDropout",
                table: "BeneficiaryFamilyMembers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "AppliancesSummary",
                table: "Beneficiaries",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DebtCreditor",
                table: "Beneficiaries",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DebtNotes",
                table: "Beneficiaries",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ElectricitySource",
                table: "Beneficiaries",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "HasDebts",
                table: "Beneficiaries",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsCandidateForMicroProject",
                table: "Beneficiaries",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsThreatenedWithImprisonment",
                table: "Beneficiaries",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PriorityTier",
                table: "Beneficiaries",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RoofType",
                table: "Beneficiaries",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SanitationType",
                table: "Beneficiaries",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SuggestedMicroProject",
                table: "Beneficiaries",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "TotalDebtAmount",
                table: "Beneficiaries",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VillageOrDistrict",
                table: "Beneficiaries",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WallType",
                table: "Beneficiaries",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WaterSource",
                table: "Beneficiaries",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EducationStage",
                table: "BeneficiaryFamilyMembers");

            migrationBuilder.DropColumn(
                name: "IsChildLabor",
                table: "BeneficiaryFamilyMembers");

            migrationBuilder.DropColumn(
                name: "IsSchoolDropout",
                table: "BeneficiaryFamilyMembers");

            migrationBuilder.DropColumn(
                name: "AppliancesSummary",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "DebtCreditor",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "DebtNotes",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "ElectricitySource",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "HasDebts",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "IsCandidateForMicroProject",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "IsThreatenedWithImprisonment",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "PriorityTier",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "RoofType",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "SanitationType",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "SuggestedMicroProject",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "TotalDebtAmount",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "VillageOrDistrict",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "WallType",
                table: "Beneficiaries");

            migrationBuilder.DropColumn(
                name: "WaterSource",
                table: "Beneficiaries");
        }
    }
}
